"""Pydantic schemas for the shopping cart."""

from decimal import Decimal

from pydantic import BaseModel, Field


class CartItemAdd(BaseModel):
    product_id: int
    quantity: int = Field(gt=0)


class CartItemUpdate(BaseModel):
    quantity: int = Field(gt=0)


class CartItemOut(BaseModel):
    cart_item_id: int
    product_id: int
    product_name: str
    price: Decimal
    stock_quantity: int
    quantity: int
    subtotal: Decimal


class CartOut(BaseModel):
    cart_id: int
    items: list[CartItemOut]
    total_amount: Decimal
