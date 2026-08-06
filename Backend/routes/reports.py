"""
routes/reports.py
-------------------
The four reports Admins can view: Monthly Sales, Yearly Sales,
Product Sales, and Inventory. All of these are admin-only.

"Sales" always means completed orders - pending orders haven't been
paid for yet, and cancelled orders were never fulfilled, so neither
should count towards revenue.
"""

from datetime import date

from fastapi import APIRouter, Depends

import config
from database import get_cursor
from models.report_models import (
    DailySales,
    InventoryItem,
    InventoryReport,
    MonthlyBreakdownItem,
    MonthlySalesReport,
    ProductSalesItem,
    ProductSalesReport,
    YearlySalesReport,
)
from utils.dependencies import require_admin

router = APIRouter(prefix="/reports", tags=["Reports"])


@router.get("/monthly-sales", response_model=MonthlySalesReport)
def monthly_sales_report(
    year: int | None = None,
    month: int | None = None,
    current_admin: dict = Depends(require_admin),
):
    today = date.today()
    year = year or today.year
    month = month or today.month

    with get_cursor() as cursor:
        cursor.execute(
            """
            SELECT COALESCE(SUM(total_amount), 0) AS total_sales, COUNT(*) AS order_count
            FROM Orders
            WHERE status = 'completed' AND YEAR(order_date) = %s AND MONTH(order_date) = %s
            """,
            (year, month),
        )
        totals = cursor.fetchone()

        cursor.execute(
            """
            SELECT DAY(order_date) AS day, SUM(total_amount) AS total_sales, COUNT(*) AS order_count
            FROM Orders
            WHERE status = 'completed' AND YEAR(order_date) = %s AND MONTH(order_date) = %s
            GROUP BY DAY(order_date)
            ORDER BY day
            """,
            (year, month),
        )
        daily_breakdown = cursor.fetchall()

    return MonthlySalesReport(
        year=year,
        month=month,
        total_sales=totals["total_sales"],
        order_count=totals["order_count"],
        daily_breakdown=[DailySales(**row) for row in daily_breakdown],
    )


@router.get("/yearly-sales", response_model=YearlySalesReport)
def yearly_sales_report(year: int | None = None, current_admin: dict = Depends(require_admin)):
    year = year or date.today().year

    with get_cursor() as cursor:
        cursor.execute(
            """
            SELECT COALESCE(SUM(total_amount), 0) AS total_sales, COUNT(*) AS order_count
            FROM Orders
            WHERE status = 'completed' AND YEAR(order_date) = %s
            """,
            (year,),
        )
        totals = cursor.fetchone()

        cursor.execute(
            """
            SELECT MONTH(order_date) AS month, SUM(total_amount) AS total_sales, COUNT(*) AS order_count
            FROM Orders
            WHERE status = 'completed' AND YEAR(order_date) = %s
            GROUP BY MONTH(order_date)
            """,
            (year,),
        )
        rows_by_month = {row["month"]: row for row in cursor.fetchall()}

    # Fill in every month 1-12, even ones with no sales, so the chart
    # / table on the frontend always has 12 rows to show.
    monthly_breakdown = [
        MonthlyBreakdownItem(
            month=m,
            total_sales=rows_by_month[m]["total_sales"] if m in rows_by_month else 0,
            order_count=rows_by_month[m]["order_count"] if m in rows_by_month else 0,
        )
        for m in range(1, 13)
    ]

    return YearlySalesReport(
        year=year,
        total_sales=totals["total_sales"],
        order_count=totals["order_count"],
        monthly_breakdown=monthly_breakdown,
    )


@router.get("/product-sales", response_model=ProductSalesReport)
def product_sales_report(current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute(
            """
            SELECT oi.product_id, oi.product_name,
                   SUM(oi.quantity) AS quantity_sold,
                   SUM(oi.subtotal) AS total_revenue
            FROM OrderItems oi
            JOIN Orders o ON o.order_id = oi.order_id
            WHERE o.status = 'completed'
            GROUP BY oi.product_id, oi.product_name
            ORDER BY total_revenue DESC
            """
        )
        items = cursor.fetchall()

    return ProductSalesReport(items=[ProductSalesItem(**row) for row in items])


@router.get("/inventory", response_model=InventoryReport)
def inventory_report(current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute(
            """
            SELECT p.product_id, p.product_name, c.category_name, p.price,
                   p.stock_quantity, p.is_active
            FROM Products p
            JOIN Categories c ON c.category_id = p.category_id
            ORDER BY p.stock_quantity ASC
            """
        )
        rows = cursor.fetchall()

    items = [
        InventoryItem(
            product_id=row["product_id"],
            product_name=row["product_name"],
            category_name=row["category_name"],
            price=row["price"],
            stock_quantity=row["stock_quantity"],
            is_low_stock=row["stock_quantity"] <= config.LOW_STOCK_THRESHOLD,
            is_active=row["is_active"],
        )
        for row in rows
    ]
    return InventoryReport(items=items, low_stock_threshold=config.LOW_STOCK_THRESHOLD)


@router.get("/activity-logs")
def activity_logs(limit: int = 50, current_admin: dict = Depends(require_admin)):
    """Bonus report: a simple audit trail. Not required by the
    project spec, but the ActivityLogs table is only useful if
    something lets you actually read it."""
    with get_cursor() as cursor:
        cursor.execute(
            "SELECT log_id, user_type, user_id, action, details, created_at FROM ActivityLogs ORDER BY created_at DESC LIMIT %s",
            (limit,),
        )
        return cursor.fetchall()
