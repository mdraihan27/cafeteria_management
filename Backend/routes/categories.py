"""
routes/categories.py
---------------------
Product categories (e.g. "Beverages", "Desserts").

Reading the category list is public (both the admin app and the
customer app need it to show the menu). Creating, editing, and
deleting categories is restricted to Admins.
"""

from fastapi import APIRouter, Depends, HTTPException, status

from database import get_cursor
from models.category_models import CategoryCreate, CategoryOut, CategoryUpdate
from utils.activity_logger import log_activity
from utils.dependencies import require_admin

router = APIRouter(prefix="/categories", tags=["Categories"])


@router.get("", response_model=list[CategoryOut])
def list_categories():
    with get_cursor() as cursor:
        cursor.execute(
            """
            SELECT c.category_id, c.category_name, c.description, c.created_at,
                   COUNT(p.product_id) AS product_count
            FROM Categories c
            LEFT JOIN Products p ON p.category_id = c.category_id AND p.is_active = 1
            GROUP BY c.category_id
            ORDER BY c.category_name
            """
        )
        return cursor.fetchall()


@router.get("/{category_id}", response_model=CategoryOut)
def get_category(category_id: int):
    with get_cursor() as cursor:
        cursor.execute(
            """
            SELECT c.category_id, c.category_name, c.description, c.created_at,
                   COUNT(p.product_id) AS product_count
            FROM Categories c
            LEFT JOIN Products p ON p.category_id = c.category_id AND p.is_active = 1
            WHERE c.category_id = %s
            GROUP BY c.category_id
            """,
            (category_id,),
        )
        category = cursor.fetchone()
    if category is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Category not found")
    return category


@router.post("", response_model=CategoryOut, status_code=status.HTTP_201_CREATED)
def create_category(payload: CategoryCreate, current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute("SELECT category_id FROM Categories WHERE category_name = %s", (payload.category_name,))
        if cursor.fetchone():
            raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail="A category with this name already exists")

    with get_cursor(commit=True) as cursor:
        cursor.execute(
            "INSERT INTO Categories (category_name, description) VALUES (%s, %s)",
            (payload.category_name, payload.description),
        )
        new_id = cursor.lastrowid

    log_activity("admin", current_admin["admin_id"], "CATEGORY_CREATED", f"Created category '{payload.category_name}'")
    return get_category(new_id)


@router.put("/{category_id}", response_model=CategoryOut)
def update_category(category_id: int, payload: CategoryUpdate, current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute("SELECT category_id FROM Categories WHERE category_id = %s", (category_id,))
        if cursor.fetchone() is None:
            raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Category not found")

        cursor.execute(
            "SELECT category_id FROM Categories WHERE category_name = %s AND category_id != %s",
            (payload.category_name, category_id),
        )
        if cursor.fetchone():
            raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail="A category with this name already exists")

    with get_cursor(commit=True) as cursor:
        cursor.execute(
            "UPDATE Categories SET category_name = %s, description = %s WHERE category_id = %s",
            (payload.category_name, payload.description, category_id),
        )

    log_activity("admin", current_admin["admin_id"], "CATEGORY_UPDATED", f"Updated category #{category_id}")
    return get_category(category_id)


@router.delete("/{category_id}")
def delete_category(category_id: int, current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute("SELECT category_id FROM Categories WHERE category_id = %s", (category_id,))
        if cursor.fetchone() is None:
            raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Category not found")

        cursor.execute("SELECT COUNT(*) AS count FROM Products WHERE category_id = %s", (category_id,))
        product_count = cursor.fetchone()["count"]

    if product_count > 0:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Cannot delete this category - {product_count} product(s) still belong to it. "
                   f"Move or delete those products first.",
        )

    with get_cursor(commit=True) as cursor:
        cursor.execute("DELETE FROM Categories WHERE category_id = %s", (category_id,))

    log_activity("admin", current_admin["admin_id"], "CATEGORY_DELETED", f"Deleted category #{category_id}")
    return {"message": "Category deleted successfully"}
