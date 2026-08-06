"""
utils/dependencies.py
----------------------
FastAPI "dependencies" that protect routes behind a login check.

Add `current_admin: dict = Depends(require_admin)` (or
`require_customer`) as a parameter to any route function, and FastAPI
will automatically:
  1. Read the "Authorization: Bearer <token>" header.
  2. Verify the JWT is valid and not expired.
  3. Check the token belongs to the right role (admin or customer).
  4. Look up the current row in the database and hand it to your
     route function as a plain dict.

If any of that fails, the request is rejected with HTTP 401 before
your route code even runs.
"""

import jwt
from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer

from database import get_cursor
from utils.security import decode_access_token

# This tells FastAPI's auto-generated docs (Swagger UI) that routes
# using it need a Bearer token, and it extracts that token for us.
_bearer_scheme = HTTPBearer()


def _decode_token_or_401(credentials: HTTPAuthorizationCredentials) -> dict:
    try:
        return decode_access_token(credentials.credentials)
    except jwt.ExpiredSignatureError:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Session expired, please log in again")
    except jwt.PyJWTError:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid authentication token")


def require_admin(credentials: HTTPAuthorizationCredentials = Depends(_bearer_scheme)) -> dict:
    """Use this dependency on routes that only Admins may call."""
    payload = _decode_token_or_401(credentials)
    if payload.get("role") != "admin":
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Admin access required")

    admin_id = int(payload["sub"])
    with get_cursor() as cursor:
        cursor.execute(
            "SELECT admin_id, full_name, username, email, created_at FROM Admins WHERE admin_id = %s",
            (admin_id,),
        )
        admin = cursor.fetchone()

    if admin is None:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Admin account no longer exists")
    return admin


def require_customer(credentials: HTTPAuthorizationCredentials = Depends(_bearer_scheme)) -> dict:
    """Use this dependency on routes that only Customers may call."""
    payload = _decode_token_or_401(credentials)
    if payload.get("role") != "customer":
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Customer access required")

    customer_id = int(payload["sub"])
    with get_cursor() as cursor:
        cursor.execute(
            "SELECT customer_id, full_name, email, phone, address, created_at FROM Customers WHERE customer_id = %s",
            (customer_id,),
        )
        customer = cursor.fetchone()

    if customer is None:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Customer account no longer exists")
    return customer
