"""Pydantic schemas for the four admin reports."""

from decimal import Decimal

from pydantic import BaseModel


class DailySales(BaseModel):
    day: int
    total_sales: Decimal
    order_count: int


class MonthlySalesReport(BaseModel):
    year: int
    month: int
    total_sales: Decimal
    order_count: int
    daily_breakdown: list[DailySales]


class MonthlyBreakdownItem(BaseModel):
    month: int
    total_sales: Decimal
    order_count: int


class YearlySalesReport(BaseModel):
    year: int
    total_sales: Decimal
    order_count: int
    monthly_breakdown: list[MonthlyBreakdownItem]


class ProductSalesItem(BaseModel):
    product_id: int | None = None
    product_name: str
    quantity_sold: int
    total_revenue: Decimal


class ProductSalesReport(BaseModel):
    items: list[ProductSalesItem]


class InventoryItem(BaseModel):
    product_id: int
    product_name: str
    category_name: str | None = None
    price: Decimal
    stock_quantity: int
    is_low_stock: bool
    is_active: bool


class InventoryReport(BaseModel):
    items: list[InventoryItem]
    low_stock_threshold: int
