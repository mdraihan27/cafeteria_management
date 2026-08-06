"""Pydantic schemas for orders, order items, and payments."""

from datetime import datetime
from decimal import Decimal

from pydantic import BaseModel, Field


class OrderItemOut(BaseModel):
    order_item_id: int
    product_id: int | None = None
    product_name: str
    unit_price: Decimal
    quantity: int
    subtotal: Decimal


class PaymentOut(BaseModel):
    payment_id: int
    amount: Decimal
    payment_method: str
    payment_status: str
    payment_date: datetime


class OrderOut(BaseModel):
    order_id: int
    customer_id: int
    customer_name: str | None = None
    order_date: datetime
    status: str
    total_amount: Decimal
    items: list[OrderItemOut] = []
    payment: PaymentOut | None = None


class CheckoutRequest(BaseModel):
    """Body is optional - checkout always uses whatever is currently
    in the customer's cart. Included so the frontend can add an order
    note in the future without changing the endpoint shape."""
    note: str | None = Field(default=None, max_length=255)


class OrderStatusUpdate(BaseModel):
    status: str = Field(description="One of: completed, cancelled")
    payment_method: str | None = Field(
        default="cash",
        description="Required (implicitly) when status is 'completed'. One of: cash, card, mobile_banking",
    )
