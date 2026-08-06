"""
routes/orders.py
------------------
Placing, viewing, cancelling, and managing orders.

Business rules enforced here:
  - Every order must contain at least one product (checkout fails if
    the cart is empty).
  - Products with zero stock cannot be ordered (re-checked at
    checkout, even though the cart screen already checks this too).
  - Stock quantity can never go negative.
  - Customers can only cancel orders that are still "pending".
  - Every completed order must have a Payment record - creating one
    is a required part of marking an order "completed".
  - Administrators can manage (view / complete / cancel) all orders.
  - Order history keeps working even if a product is later deleted,
    because OrderItems stores a product_name/unit_price snapshot.
"""

from fastapi import APIRouter, Depends, HTTPException, status

from database import get_cursor
from models.order_models import CheckoutRequest, OrderOut, OrderStatusUpdate
from utils.activity_logger import log_activity
from utils.dependencies import require_admin, require_customer

router = APIRouter(prefix="/orders", tags=["Orders"])

_VALID_PAYMENT_METHODS = {"cash", "card", "mobile_banking"}


def fetch_order_detail(order_id: int) -> dict | None:
    with get_cursor() as cursor:
        cursor.execute(
            """
            SELECT o.order_id, o.customer_id, cu.full_name AS customer_name,
                   o.order_date, o.status, o.total_amount
            FROM Orders o
            JOIN Customers cu ON cu.customer_id = o.customer_id
            WHERE o.order_id = %s
            """,
            (order_id,),
        )
        order = cursor.fetchone()
        if order is None:
            return None

        cursor.execute(
            """
            SELECT order_item_id, product_id, product_name, unit_price, quantity, subtotal
            FROM OrderItems WHERE order_id = %s ORDER BY order_item_id
            """,
            (order_id,),
        )
        order["items"] = cursor.fetchall()

        cursor.execute(
            "SELECT payment_id, amount, payment_method, payment_status, payment_date FROM Payments WHERE order_id = %s",
            (order_id,),
        )
        order["payment"] = cursor.fetchone()

    return order


def _restore_stock_for_order(cursor, order_id: int) -> None:
    """Give stock back for every item in an order that is being
    cancelled. Skips items whose product was later deleted."""
    cursor.execute(
        "SELECT product_id, quantity FROM OrderItems WHERE order_id = %s AND product_id IS NOT NULL",
        (order_id,),
    )
    for item in cursor.fetchall():
        cursor.execute(
            "UPDATE Products SET stock_quantity = stock_quantity + %s WHERE product_id = %s",
            (item["quantity"], item["product_id"]),
        )


# =====================================================================
# Customer endpoints
# =====================================================================

@router.post("/checkout", response_model=OrderOut, status_code=status.HTTP_201_CREATED)
def checkout(payload: CheckoutRequest, current_customer: dict = Depends(require_customer)):
    customer_id = current_customer["customer_id"]

    with get_cursor() as cursor:
        cursor.execute("SELECT cart_id FROM Cart WHERE customer_id = %s", (customer_id,))
        cart = cursor.fetchone()
        cart_items = []
        if cart:
            cursor.execute(
                """
                SELECT ci.cart_item_id, ci.product_id, ci.quantity,
                       p.product_name, p.price, p.stock_quantity, p.is_active
                FROM CartItems ci
                JOIN Products p ON p.product_id = ci.product_id
                WHERE ci.cart_id = %s
                """,
                (cart["cart_id"],),
            )
            cart_items = cursor.fetchall()

    if not cart_items:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Your cart is empty. Add at least one product before checking out.")

    for item in cart_items:
        if not item["is_active"]:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=f"'{item['product_name']}' is no longer available")
        if item["stock_quantity"] <= 0:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=f"'{item['product_name']}' is out of stock")
        if item["quantity"] > item["stock_quantity"]:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=f"Only {item['stock_quantity']} of '{item['product_name']}' left in stock")

    total_amount = sum(item["price"] * item["quantity"] for item in cart_items)

    with get_cursor(commit=True) as cursor:
        cursor.execute(
            "INSERT INTO Orders (customer_id, status, total_amount) VALUES (%s, 'pending', %s)",
            (customer_id, total_amount),
        )
        order_id = cursor.lastrowid

        for item in cart_items:
            subtotal = item["price"] * item["quantity"]
            cursor.execute(
                """
                INSERT INTO OrderItems (order_id, product_id, product_name, unit_price, quantity, subtotal)
                VALUES (%s, %s, %s, %s, %s, %s)
                """,
                (order_id, item["product_id"], item["product_name"], item["price"], item["quantity"], subtotal),
            )
            # Stock can never go negative - the WHERE clause guarantees that,
            # and we double checked availability above too.
            cursor.execute(
                "UPDATE Products SET stock_quantity = stock_quantity - %s WHERE product_id = %s AND stock_quantity >= %s",
                (item["quantity"], item["product_id"], item["quantity"]),
            )
            if cursor.rowcount == 0:
                raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=f"'{item['product_name']}' no longer has enough stock")

        cursor.execute("DELETE FROM CartItems WHERE cart_id = %s", (cart["cart_id"],))

    log_activity("customer", customer_id, "ORDER_PLACED", f"Placed order #{order_id} (total {total_amount})")
    return fetch_order_detail(order_id)


@router.get("/my", response_model=list[OrderOut])
def get_my_orders(current_customer: dict = Depends(require_customer)):
    """Returns the customer's full order history, most recent first.
    The frontend uses this both for the Dashboard's "Recent Orders"
    (it just shows the first few) and the full "Order History" screen.
    """
    with get_cursor() as cursor:
        cursor.execute(
            "SELECT order_id FROM Orders WHERE customer_id = %s ORDER BY order_date DESC",
            (current_customer["customer_id"],),
        )
        order_ids = [row["order_id"] for row in cursor.fetchall()]

    return [fetch_order_detail(order_id) for order_id in order_ids]


@router.get("/my/{order_id}", response_model=OrderOut)
def get_my_order_detail(order_id: int, current_customer: dict = Depends(require_customer)):
    order = fetch_order_detail(order_id)
    if order is None or order["customer_id"] != current_customer["customer_id"]:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Order not found")
    return order


@router.post("/my/{order_id}/cancel", response_model=OrderOut)
def cancel_my_order(order_id: int, current_customer: dict = Depends(require_customer)):
    order = fetch_order_detail(order_id)
    if order is None or order["customer_id"] != current_customer["customer_id"]:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Order not found")
    if order["status"] != "pending":
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Only pending orders can be cancelled")

    with get_cursor(commit=True) as cursor:
        _restore_stock_for_order(cursor, order_id)
        cursor.execute("UPDATE Orders SET status = 'cancelled' WHERE order_id = %s", (order_id,))

    log_activity("customer", current_customer["customer_id"], "ORDER_CANCELLED", f"Cancelled order #{order_id}")
    return fetch_order_detail(order_id)


# =====================================================================
# Admin endpoints - administrators can manage all orders
# =====================================================================

@router.get("", response_model=list[OrderOut])
def list_all_orders(status_filter: str | None = None, current_admin: dict = Depends(require_admin)):
    query = "SELECT order_id FROM Orders"
    params: list = []
    if status_filter:
        query += " WHERE status = %s"
        params.append(status_filter)
    query += " ORDER BY order_date DESC"

    with get_cursor() as cursor:
        cursor.execute(query, tuple(params))
        order_ids = [row["order_id"] for row in cursor.fetchall()]

    return [fetch_order_detail(order_id) for order_id in order_ids]


@router.get("/{order_id}", response_model=OrderOut)
def get_order_detail(order_id: int, current_admin: dict = Depends(require_admin)):
    order = fetch_order_detail(order_id)
    if order is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Order not found")
    return order


@router.put("/{order_id}/status", response_model=OrderOut)
def update_order_status(order_id: int, payload: OrderStatusUpdate, current_admin: dict = Depends(require_admin)):
    order = fetch_order_detail(order_id)
    if order is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Order not found")
    if order["status"] != "pending":
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Only pending orders can change status")
    if payload.status not in ("completed", "cancelled"):
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Status must be 'completed' or 'cancelled'")

    if payload.status == "cancelled":
        with get_cursor(commit=True) as cursor:
            _restore_stock_for_order(cursor, order_id)
            cursor.execute("UPDATE Orders SET status = 'cancelled' WHERE order_id = %s", (order_id,))
        log_activity("admin", current_admin["admin_id"], "ORDER_CANCELLED", f"Admin cancelled order #{order_id}")

    else:  # completed
        payment_method = payload.payment_method or "cash"
        if payment_method not in _VALID_PAYMENT_METHODS:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Invalid payment method")

        with get_cursor(commit=True) as cursor:
            cursor.execute("UPDATE Orders SET status = 'completed' WHERE order_id = %s", (order_id,))
            # Every completed order must have a payment record.
            cursor.execute(
                "INSERT INTO Payments (order_id, amount, payment_method, payment_status) VALUES (%s, %s, %s, 'paid')",
                (order_id, order["total_amount"], payment_method),
            )
        log_activity("admin", current_admin["admin_id"], "ORDER_COMPLETED", f"Admin marked order #{order_id} completed")

    return fetch_order_detail(order_id)
