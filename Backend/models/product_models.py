"""Pydantic schemas for menu products."""

from datetime import datetime
from decimal import Decimal

from pydantic import BaseModel, Field


class ProductCreate(BaseModel):
    product_name: str = Field(min_length=1, max_length=150)
    description: str | None = Field(default=None, max_length=500)
    price: Decimal = Field(gt=0)
    stock_quantity: int = Field(ge=0)
    category_id: int


class ProductUpdate(BaseModel):
    product_name: str = Field(min_length=1, max_length=150)
    description: str | None = Field(default=None, max_length=500)
    price: Decimal = Field(gt=0)
    stock_quantity: int = Field(ge=0)
    category_id: int
    is_active: bool = True


class ProductOut(BaseModel):
    product_id: int
    product_name: str
    description: str | None = None
    price: Decimal
    stock_quantity: int
    category_id: int
    category_name: str | None = None
    is_active: bool
    created_at: datetime
    updated_at: datetime
