"""
routes/cart.py
---------------
Shopping cart management for Customers. A cart is created
automatically for every customer at registration time (see
routes/auth.py), so these endpoints assume one already exists.

Business rules enforced here:
  - Products with zero stock cannot be added to the cart.
  - You can never add/set more of a product than is currently in
    stock.
"""

from fastapi import APIRouter, Depends, HTTPException, status

from database import get_cursor
from models.cart_models import CartItemAdd, CartItemUpdate, CartOut
from utils.dependencies import require_customer

router = APIRouter(prefix="/cart", tags=["Cart"])


def _get_or_create_cart_id(customer_id: int) -> int:
    with get_cursor() as cursor:
        cursor.execute("SELECT cart_id FROM Cart WHERE customer_id = %s", (customer_id,))
        row = cursor.fetchone()
    if row:
        return row["cart_id"]

    with get_cursor(commit=True) as cursor:
        cursor.execute("INSERT INTO Cart (customer_id) VALUES (%s)", (customer_id,))
        return cursor.lastrowid


def _build_cart_response(cart_id: int) -> CartOut:
    with get_cursor() as cursor:
        cursor.execute(
            """
            SELECT ci.cart_item_id, ci.product_id, p.product_name, p.price,
                   p.stock_quantity, ci.quantity, (p.price * ci.quantity) AS subtotal
            FROM CartItems ci
            JOIN Products p ON p.product_id = ci.product_id
            WHERE ci.cart_id = %s
            ORDER BY ci.cart_item_id
            """,
            (cart_id,),
        )
        items = cursor.fetchall()

    total_amount = sum((item["subtotal"] for item in items), start=0)
    return CartOut(cart_id=cart_id, items=items, total_amount=total_amount)


@router.get("", response_model=CartOut)
def get_cart(current_customer: dict = Depends(require_customer)):
    cart_id = _get_or_create_cart_id(current_customer["customer_id"])
    return _build_cart_response(cart_id)


@router.post("/items", response_model=CartOut, status_code=status.HTTP_201_CREATED)
def add_item_to_cart(payload: CartItemAdd, current_customer: dict = Depends(require_customer)):
    cart_id = _get_or_create_cart_id(current_customer["customer_id"])

    with get_cursor() as cursor:
        cursor.execute(
            "SELECT stock_quantity, is_active FROM Products WHERE product_id = %s",
            (payload.product_id,),
        )
        product = cursor.fetchone()
    if product is None or not product["is_active"]:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Product not found")
    if product["stock_quantity"] <= 0:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="This product is out of stock")

    with get_cursor() as cursor:
        cursor.execute(
            "SELECT cart_item_id, quantity FROM CartItems WHERE cart_id = %s AND product_id = %s",
            (cart_id, payload.product_id),
        )
        existing = cursor.fetchone()

    new_quantity = payload.quantity + (existing["quantity"] if existing else 0)
    if new_quantity > product["stock_quantity"]:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Only {product['stock_quantity']} left in stock",
        )

    with get_cursor(commit=True) as cursor:
        if existing:
            cursor.execute(
                "UPDATE CartItems SET quantity = %s WHERE cart_item_id = %s",
                (new_quantity, existing["cart_item_id"]),
            )
        else:
            cursor.execute(
                "INSERT INTO CartItems (cart_id, product_id, quantity) VALUES (%s, %s, %s)",
                (cart_id, payload.product_id, payload.quantity),
            )

    return _build_cart_response(cart_id)


@router.put("/items/{cart_item_id}", response_model=CartOut)
def update_cart_item(cart_item_id: int, payload: CartItemUpdate, current_customer: dict = Depends(require_customer)):
    cart_id = _get_or_create_cart_id(current_customer["customer_id"])

    with get_cursor() as cursor:
        cursor.execute(
            """
            SELECT ci.cart_item_id, p.stock_quantity
            FROM CartItems ci
            JOIN Products p ON p.product_id = ci.product_id
            WHERE ci.cart_item_id = %s AND ci.cart_id = %s
            """,
            (cart_item_id, cart_id),
        )
        item = cursor.fetchone()
    if item is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Cart item not found")
    if payload.quantity > item["stock_quantity"]:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Only {item['stock_quantity']} left in stock",
        )

    with get_cursor(commit=True) as cursor:
        cursor.execute("UPDATE CartItems SET quantity = %s WHERE cart_item_id = %s", (payload.quantity, cart_item_id))

    return _build_cart_response(cart_id)


@router.delete("/items/{cart_item_id}", response_model=CartOut)
def remove_cart_item(cart_item_id: int, current_customer: dict = Depends(require_customer)):
    cart_id = _get_or_create_cart_id(current_customer["customer_id"])

    with get_cursor(commit=True) as cursor:
        cursor.execute("DELETE FROM CartItems WHERE cart_item_id = %s AND cart_id = %s", (cart_item_id, cart_id))
        if cursor.rowcount == 0:
            raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Cart item not found")

    return _build_cart_response(cart_id)


@router.delete("", response_model=CartOut)
def clear_cart(current_customer: dict = Depends(require_customer)):
    cart_id = _get_or_create_cart_id(current_customer["customer_id"])
    with get_cursor(commit=True) as cursor:
        cursor.execute("DELETE FROM CartItems WHERE cart_id = %s", (cart_id,))
    return _build_cart_response(cart_id)
