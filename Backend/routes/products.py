"""
routes/products.py
-------------------
Menu products.

Business rules enforced here:
  - Product names must be unique.
  - Stock quantity can never go negative (also enforced by a CHECK
    constraint in the database as a safety net).
  - "Deleting" a product is a soft delete (is_active = 0) so that old
    orders can still show the product's name and price.
"""

from fastapi import APIRouter, Depends, HTTPException, status

from database import get_cursor
from models.product_models import ProductCreate, ProductOut, ProductUpdate
from utils.activity_logger import log_activity
from utils.dependencies import require_admin

router = APIRouter(prefix="/products", tags=["Products"])

_SELECT_PRODUCT = """
    SELECT p.product_id, p.product_name, p.description, p.price, p.stock_quantity,
           p.category_id, c.category_name, p.is_active, p.created_at, p.updated_at
    FROM Products p
    JOIN Categories c ON c.category_id = p.category_id
"""


@router.get("", response_model=list[ProductOut])
def list_products(category_id: int | None = None, search: str | None = None):
    """Public product listing - only shows active (not deleted) products.
    Used by the customer app's menu screen, and the admin app's
    read-only views."""
    query = _SELECT_PRODUCT + " WHERE p.is_active = 1"
    params: list = []

    if category_id is not None:
        query += " AND p.category_id = %s"
        params.append(category_id)
    if search:
        query += " AND p.product_name LIKE %s"
        params.append(f"%{search}%")

    query += " ORDER BY p.product_name"

    with get_cursor() as cursor:
        cursor.execute(query, tuple(params))
        return cursor.fetchall()


@router.get("/admin/all", response_model=list[ProductOut])
def list_all_products_for_admin(current_admin: dict = Depends(require_admin)):
    """Admin product management screen needs to see inactive
    (soft-deleted) products too, so they can be reactivated."""
    with get_cursor() as cursor:
        cursor.execute(_SELECT_PRODUCT + " ORDER BY p.product_name")
        return cursor.fetchall()


@router.get("/{product_id}", response_model=ProductOut)
def get_product(product_id: int):
    with get_cursor() as cursor:
        cursor.execute(_SELECT_PRODUCT + " WHERE p.product_id = %s", (product_id,))
        product = cursor.fetchone()
    if product is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Product not found")
    return product


@router.post("", response_model=ProductOut, status_code=status.HTTP_201_CREATED)
def create_product(payload: ProductCreate, current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute("SELECT category_id FROM Categories WHERE category_id = %s", (payload.category_id,))
        if cursor.fetchone() is None:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Category does not exist")

        cursor.execute("SELECT product_id FROM Products WHERE product_name = %s", (payload.product_name,))
        if cursor.fetchone():
            raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail="A product with this name already exists")

    with get_cursor(commit=True) as cursor:
        cursor.execute(
            """
            INSERT INTO Products (product_name, description, price, stock_quantity, category_id)
            VALUES (%s, %s, %s, %s, %s)
            """,
            (payload.product_name, payload.description, payload.price, payload.stock_quantity, payload.category_id),
        )
        new_id = cursor.lastrowid

    log_activity("admin", current_admin["admin_id"], "PRODUCT_CREATED", f"Created product '{payload.product_name}'")
    return get_product(new_id)


@router.put("/{product_id}", response_model=ProductOut)
def update_product(product_id: int, payload: ProductUpdate, current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute("SELECT product_id FROM Products WHERE product_id = %s", (product_id,))
        if cursor.fetchone() is None:
            raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Product not found")

        cursor.execute("SELECT category_id FROM Categories WHERE category_id = %s", (payload.category_id,))
        if cursor.fetchone() is None:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Category does not exist")

        cursor.execute(
            "SELECT product_id FROM Products WHERE product_name = %s AND product_id != %s",
            (payload.product_name, product_id),
        )
        if cursor.fetchone():
            raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail="A product with this name already exists")

    with get_cursor(commit=True) as cursor:
        cursor.execute(
            """
            UPDATE Products
            SET product_name = %s, description = %s, price = %s, stock_quantity = %s,
                category_id = %s, is_active = %s
            WHERE product_id = %s
            """,
            (
                payload.product_name, payload.description, payload.price, payload.stock_quantity,
                payload.category_id, payload.is_active, product_id,
            ),
        )

    log_activity("admin", current_admin["admin_id"], "PRODUCT_UPDATED", f"Updated product #{product_id}")
    return get_product(product_id)


@router.delete("/{product_id}")
def delete_product(product_id: int, current_admin: dict = Depends(require_admin)):
    """Soft delete - keeps the row (and all past order history that
    references it) but hides it from the customer-facing menu."""
    with get_cursor() as cursor:
        cursor.execute("SELECT product_id FROM Products WHERE product_id = %s", (product_id,))
        if cursor.fetchone() is None:
            raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Product not found")

    with get_cursor(commit=True) as cursor:
        cursor.execute("UPDATE Products SET is_active = 0 WHERE product_id = %s", (product_id,))

    log_activity("admin", current_admin["admin_id"], "PRODUCT_DELETED", f"Deleted (deactivated) product #{product_id}")
    return {"message": "Product deleted successfully"}


@router.put("/{product_id}/reactivate")
def reactivate_product(product_id: int, current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute("SELECT product_id FROM Products WHERE product_id = %s", (product_id,))
        if cursor.fetchone() is None:
            raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Product not found")

    with get_cursor(commit=True) as cursor:
        cursor.execute("UPDATE Products SET is_active = 1 WHERE product_id = %s", (product_id,))

    log_activity("admin", current_admin["admin_id"], "PRODUCT_REACTIVATED", f"Reactivated product #{product_id}")
    return {"message": "Product reactivated successfully"}
