"""Pydantic schemas for product categories."""

from datetime import datetime

from pydantic import BaseModel, Field


class CategoryCreate(BaseModel):
    category_name: str = Field(min_length=1, max_length=100)
    description: str | None = Field(default=None, max_length=255)


class CategoryUpdate(BaseModel):
    category_name: str = Field(min_length=1, max_length=100)
    description: str | None = Field(default=None, max_length=255)


class CategoryOut(BaseModel):
    category_id: int
    category_name: str
    description: str | None = None
    created_at: datetime
    product_count: int = 0
