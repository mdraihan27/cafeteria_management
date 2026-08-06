"""Pydantic schemas for customer profiles."""

from datetime import datetime

from pydantic import BaseModel, EmailStr, Field


class CustomerProfileOut(BaseModel):
    customer_id: int
    full_name: str
    email: EmailStr
    phone: str | None = None
    address: str | None = None
    created_at: datetime


class CustomerProfileUpdate(BaseModel):
    full_name: str = Field(min_length=1, max_length=100)
    phone: str | None = Field(default=None, max_length=20)
    address: str | None = Field(default=None, max_length=255)


class CustomerListItem(BaseModel):
    """Shown to admins in the customer list - no password data included."""
    customer_id: int
    full_name: str
    email: EmailStr
    phone: str | None = None
    address: str | None = None
    created_at: datetime
