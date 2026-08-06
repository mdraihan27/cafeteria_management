"""
routes/auth.py
---------------
Everything related to logging in, logging out, registering, and
managing your own account (both Admins and Customers).

Admins and Customers are stored in two separate tables, so they each
get their own login/profile endpoints below, but the logic is very
similar for both.
"""

from fastapi import APIRouter, Depends, HTTPException, status

from database import get_cursor
from models.admin_models import AdminProfileOut, AdminProfileUpdate
from models.auth_models import (
    AdminLoginRequest,
    ChangePasswordRequest,
    CustomerForgotPasswordRequest,
    CustomerLoginRequest,
    CustomerRegisterRequest,
    TokenResponse,
)
from models.customer_models import CustomerProfileOut, CustomerProfileUpdate
from utils.activity_logger import log_activity
from utils.dependencies import require_admin, require_customer
from utils.security import create_access_token, hash_password, verify_password

router = APIRouter(prefix="/auth", tags=["Authentication"])


# =====================================================================
# Admin authentication
# =====================================================================

@router.post("/admin/login", response_model=TokenResponse)
def admin_login(payload: AdminLoginRequest):
    with get_cursor() as cursor:
        cursor.execute("SELECT * FROM Admins WHERE username = %s", (payload.username,))
        admin = cursor.fetchone()

    if admin is None or not verify_password(payload.password, admin["password_hash"]):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid username or password")

    token = create_access_token(admin["admin_id"], "admin")
    log_activity("admin", admin["admin_id"], "LOGIN", "Admin logged in")
    return TokenResponse(
        access_token=token,
        role="admin",
        user_id=admin["admin_id"],
        full_name=admin["full_name"],
    )


@router.post("/admin/logout")
def admin_logout(current_admin: dict = Depends(require_admin)):
    # JWT tokens can't be "revoked" without extra infrastructure (like a
    # blacklist table), so logging out simply means the frontend
    # deletes its stored token. We still record the activity here.
    log_activity("admin", current_admin["admin_id"], "LOGOUT", "Admin logged out")
    return {"message": "Logged out successfully"}


@router.get("/admin/profile", response_model=AdminProfileOut)
def get_admin_profile(current_admin: dict = Depends(require_admin)):
    return current_admin


@router.put("/admin/profile", response_model=AdminProfileOut)
def update_admin_profile(payload: AdminProfileUpdate, current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute("SELECT admin_id FROM Admins WHERE email = %s AND admin_id != %s", (payload.email, current_admin["admin_id"]))
        if cursor.fetchone():
            raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail="Email already in use")

    with get_cursor(commit=True) as cursor:
        cursor.execute(
            "UPDATE Admins SET full_name = %s, email = %s WHERE admin_id = %s",
            (payload.full_name, payload.email, current_admin["admin_id"]),
        )
        cursor.execute("SELECT admin_id, full_name, username, email, created_at FROM Admins WHERE admin_id = %s", (current_admin["admin_id"],))
        updated = cursor.fetchone()

    log_activity("admin", current_admin["admin_id"], "PROFILE_UPDATED", "Admin updated their profile")
    return updated


@router.post("/admin/change-password")
def change_admin_password(payload: ChangePasswordRequest, current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute("SELECT password_hash FROM Admins WHERE admin_id = %s", (current_admin["admin_id"],))
        row = cursor.fetchone()

    if row is None or not verify_password(payload.current_password, row["password_hash"]):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Current password is incorrect")

    new_hash = hash_password(payload.new_password)
    with get_cursor(commit=True) as cursor:
        cursor.execute("UPDATE Admins SET password_hash = %s WHERE admin_id = %s", (new_hash, current_admin["admin_id"]))

    log_activity("admin", current_admin["admin_id"], "PASSWORD_CHANGED", "Admin changed their password")
    return {"message": "Password changed successfully"}


# =====================================================================
# Customer authentication
# =====================================================================

@router.post("/customer/register", response_model=TokenResponse, status_code=status.HTTP_201_CREATED)
def customer_register(payload: CustomerRegisterRequest):
    with get_cursor() as cursor:
        cursor.execute("SELECT customer_id FROM Customers WHERE email = %s", (payload.email,))
        if cursor.fetchone():
            raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail="An account with this email already exists")

    password_hash = hash_password(payload.password)
    with get_cursor(commit=True) as cursor:
        cursor.execute(
            """
            INSERT INTO Customers (full_name, email, password_hash, phone, address)
            VALUES (%s, %s, %s, %s, %s)
            """,
            (payload.full_name, payload.email, password_hash, payload.phone, payload.address),
        )
        new_customer_id = cursor.lastrowid
        # Every customer gets an empty cart ready to use.
        cursor.execute("INSERT INTO Cart (customer_id) VALUES (%s)", (new_customer_id,))

    log_activity("customer", new_customer_id, "REGISTER", "New customer account created")
    token = create_access_token(new_customer_id, "customer")
    return TokenResponse(access_token=token, role="customer", user_id=new_customer_id, full_name=payload.full_name)


@router.post("/customer/login", response_model=TokenResponse)
def customer_login(payload: CustomerLoginRequest):
    with get_cursor() as cursor:
        cursor.execute("SELECT * FROM Customers WHERE email = %s", (payload.email,))
        customer = cursor.fetchone()

    if customer is None or not verify_password(payload.password, customer["password_hash"]):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid email or password")

    token = create_access_token(customer["customer_id"], "customer")
    log_activity("customer", customer["customer_id"], "LOGIN", "Customer logged in")
    return TokenResponse(
        access_token=token,
        role="customer",
        user_id=customer["customer_id"],
        full_name=customer["full_name"],
    )


@router.post("/customer/logout")
def customer_logout(current_customer: dict = Depends(require_customer)):
    log_activity("customer", current_customer["customer_id"], "LOGOUT", "Customer logged out")
    return {"message": "Logged out successfully"}


@router.get("/customer/profile", response_model=CustomerProfileOut)
def get_customer_profile(current_customer: dict = Depends(require_customer)):
    return current_customer


@router.put("/customer/profile", response_model=CustomerProfileOut)
def update_customer_profile(payload: CustomerProfileUpdate, current_customer: dict = Depends(require_customer)):
    with get_cursor(commit=True) as cursor:
        cursor.execute(
            "UPDATE Customers SET full_name = %s, phone = %s, address = %s WHERE customer_id = %s",
            (payload.full_name, payload.phone, payload.address, current_customer["customer_id"]),
        )
        cursor.execute(
            "SELECT customer_id, full_name, email, phone, address, created_at FROM Customers WHERE customer_id = %s",
            (current_customer["customer_id"],),
        )
        updated = cursor.fetchone()

    log_activity("customer", current_customer["customer_id"], "PROFILE_UPDATED", "Customer updated their profile")
    return updated


@router.post("/customer/change-password")
def change_customer_password(payload: ChangePasswordRequest, current_customer: dict = Depends(require_customer)):
    with get_cursor() as cursor:
        cursor.execute("SELECT password_hash FROM Customers WHERE customer_id = %s", (current_customer["customer_id"],))
        row = cursor.fetchone()

    if row is None or not verify_password(payload.current_password, row["password_hash"]):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Current password is incorrect")

    new_hash = hash_password(payload.new_password)
    with get_cursor(commit=True) as cursor:
        cursor.execute("UPDATE Customers SET password_hash = %s WHERE customer_id = %s", (new_hash, current_customer["customer_id"]))

    log_activity("customer", current_customer["customer_id"], "PASSWORD_CHANGED", "Customer changed their password")
    return {"message": "Password changed successfully"}


@router.post("/customer/forgot-password")
def customer_forgot_password(payload: CustomerForgotPasswordRequest):
    """No email server is set up for this project, so identity is
    confirmed using the email + phone number on file instead of an
    emailed reset link."""
    with get_cursor() as cursor:
        cursor.execute(
            "SELECT customer_id FROM Customers WHERE email = %s AND phone = %s",
            (payload.email, payload.phone),
        )
        customer = cursor.fetchone()

    if customer is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="No account matches that email and phone number",
        )

    new_hash = hash_password(payload.new_password)
    with get_cursor(commit=True) as cursor:
        cursor.execute("UPDATE Customers SET password_hash = %s WHERE customer_id = %s", (new_hash, customer["customer_id"]))

    log_activity("customer", customer["customer_id"], "PASSWORD_RESET", "Customer reset their password via forgot-password")
    return {"message": "Password reset successfully. You can now log in with your new password."}
