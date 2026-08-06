"""
routes/dashboard.py
---------------------
The "at a glance" screen each role sees right after logging in.
"""

from fastapi import APIRouter, Depends

import config
from database import get_cursor
from models.dashboard_models import AdminDashboardOut, CustomerDashboardOut
from routes.orders import fetch_order_detail
from utils.dependencies import require_admin, require_customer

router = APIRouter(prefix="/dashboard", tags=["Dashboard"])


@router.get("/admin", response_model=AdminDashboardOut)
def admin_dashboard(current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute("SELECT COUNT(*) AS n FROM Products WHERE is_active = 1")
        total_products = cursor.fetchone()["n"]

        cursor.execute("SELECT COUNT(*) AS n FROM Customers")
        total_customers = cursor.fetchone()["n"]

        cursor.execute("SELECT COUNT(*) AS n FROM Orders")
        total_orders = cursor.fetchone()["n"]

        cursor.execute(
            """
            SELECT COALESCE(SUM(total_amount), 0) AS total
            FROM Orders
            WHERE status = 'completed'
              AND YEAR(order_date) = YEAR(CURDATE())
              AND MONTH(order_date) = MONTH(CURDATE())
            """
        )
        monthly_sales = cursor.fetchone()["total"]

        cursor.execute(
            """
            SELECT p.product_id, p.product_name, p.description, p.price, p.stock_quantity,
                   p.category_id, c.category_name, p.is_active, p.created_at, p.updated_at
            FROM Products p
            JOIN Categories c ON c.category_id = p.category_id
            WHERE p.is_active = 1 AND p.stock_quantity <= %s
            ORDER BY p.stock_quantity ASC
            """,
            (config.LOW_STOCK_THRESHOLD,),
        )
        low_stock_products = cursor.fetchall()

    return AdminDashboardOut(
        total_products=total_products,
        total_customers=total_customers,
        total_orders=total_orders,
        monthly_sales=monthly_sales,
        low_stock_products=low_stock_products,
    )


@router.get("/customer", response_model=CustomerDashboardOut)
def customer_dashboard(current_customer: dict = Depends(require_customer)):
    customer_id = current_customer["customer_id"]

    with get_cursor() as cursor:
        cursor.execute(
            "SELECT order_id FROM Orders WHERE customer_id = %s ORDER BY order_date DESC LIMIT 5",
            (customer_id,),
        )
        recent_order_ids = [row["order_id"] for row in cursor.fetchall()]

        cursor.execute("SELECT COUNT(*) AS n FROM Orders WHERE customer_id = %s", (customer_id,))
        total_orders = cursor.fetchone()["n"]

    recent_orders = [fetch_order_detail(order_id) for order_id in recent_order_ids]
    return CustomerDashboardOut(recent_orders=recent_orders, total_orders=total_orders)
