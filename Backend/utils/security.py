"""
utils/security.py
------------------
Two jobs live here:

1. Password hashing with bcrypt - we never store a plain text
   password anywhere, only its bcrypt hash.
2. Creating and reading JWT ("JSON Web Token") login tokens - this is
   how the frontend proves "I already logged in" on every request
   after the initial login, without sending the password again.
"""

from datetime import datetime, timedelta, timezone

import bcrypt
import jwt

import config


def hash_password(plain_password: str) -> str:
    """Turn a plain text password into a bcrypt hash for storage."""
    password_bytes = plain_password.encode("utf-8")
    hashed_bytes = bcrypt.hashpw(password_bytes, bcrypt.gensalt())
    return hashed_bytes.decode("utf-8")


def verify_password(plain_password: str, password_hash: str) -> bool:
    """Check a login attempt's password against the stored bcrypt hash."""
    try:
        return bcrypt.checkpw(plain_password.encode("utf-8"), password_hash.encode("utf-8"))
    except ValueError:
        # Happens if password_hash is not a valid bcrypt hash.
        return False


def create_access_token(user_id: int, role: str) -> str:
    """Create a signed JWT for a logged-in admin or customer.

    `role` is either "admin" or "customer". Every protected endpoint
    checks this role to decide whether the request is allowed.
    """
    expire_at = datetime.now(timezone.utc) + timedelta(minutes=config.JWT_EXPIRE_MINUTES)
    payload = {
        "sub": str(user_id),
        "role": role,
        "exp": expire_at,
        "iat": datetime.now(timezone.utc),
    }
    return jwt.encode(payload, config.JWT_SECRET_KEY, algorithm=config.JWT_ALGORITHM)


def decode_access_token(token: str) -> dict:
    """Decode and verify a JWT. Raises jwt.PyJWTError if it is invalid
    or expired - callers are expected to turn that into an HTTP 401.
    """
    return jwt.decode(token, config.JWT_SECRET_KEY, algorithms=[config.JWT_ALGORITHM])
