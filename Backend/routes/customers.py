"""
routes/customers.py
---------------------
Admin-only views of the customer list. Customers manage their own
profile through routes/auth.py instead (GET/PUT /auth/customer/profile).
"""

from fastapi import APIRouter, Depends, HTTPException, status

from database import get_cursor
from models.customer_models import CustomerListItem
from utils.dependencies import require_admin

router = APIRouter(prefix="/customers", tags=["Customers"])


@router.get("", response_model=list[CustomerListItem])
def list_customers(current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute(
            "SELECT customer_id, full_name, email, phone, address, created_at FROM Customers ORDER BY full_name"
        )
        return cursor.fetchall()


@router.get("/{customer_id}", response_model=CustomerListItem)
def get_customer(customer_id: int, current_admin: dict = Depends(require_admin)):
    with get_cursor() as cursor:
        cursor.execute(
            "SELECT customer_id, full_name, email, phone, address, created_at FROM Customers WHERE customer_id = %s",
            (customer_id,),
        )
        customer = cursor.fetchone()
    if customer is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Customer not found")
    return customer
