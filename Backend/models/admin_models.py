"""Pydantic schemas for the admin's own profile."""

from datetime import datetime

from pydantic import BaseModel, EmailStr, Field


class AdminProfileOut(BaseModel):
    admin_id: int
    full_name: str
    username: str
    email: EmailStr
    created_at: datetime


class AdminProfileUpdate(BaseModel):
    full_name: str = Field(min_length=1, max_length=100)
    email: EmailStr
