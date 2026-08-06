"""Pydantic schemas for the admin and customer dashboards."""

from decimal import Decimal

from pydantic import BaseModel

from models.order_models import OrderOut
from models.product_models import ProductOut


class AdminDashboardOut(BaseModel):
    total_products: int
    total_customers: int
    total_orders: int
    monthly_sales: Decimal
    low_stock_products: list[ProductOut]


class CustomerDashboardOut(BaseModel):
    recent_orders: list[OrderOut]
    total_orders: int
