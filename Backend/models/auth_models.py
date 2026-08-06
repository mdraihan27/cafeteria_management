"""Pydantic schemas for login, registration, and password management."""

from pydantic import BaseModel, EmailStr, Field


class AdminLoginRequest(BaseModel):
    username: str
    password: str


class CustomerRegisterRequest(BaseModel):
    full_name: str = Field(min_length=1, max_length=100)
    email: EmailStr
    password: str = Field(min_length=6, max_length=100)
    phone: str | None = Field(default=None, max_length=20)
    address: str | None = Field(default=None, max_length=255)


class CustomerLoginRequest(BaseModel):
    email: EmailStr
    password: str


class ChangePasswordRequest(BaseModel):
    current_password: str
    new_password: str = Field(min_length=6, max_length=100)


class CustomerForgotPasswordRequest(BaseModel):
    """Simplified "forgot password" flow with no email server involved:
    the customer proves who they are with their email + phone number
    (both were supplied at registration), then sets a new password.
    """
    email: EmailStr
    phone: str
    new_password: str = Field(min_length=6, max_length=100)


class TokenResponse(BaseModel):
    access_token: str
    token_type: str = "bearer"
    role: str
    user_id: int
    full_name: str
