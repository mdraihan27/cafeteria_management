"""
utils/activity_logger.py
-------------------------
A tiny helper that writes one row to the ActivityLogs table whenever
something noteworthy happens (login, order placed, product changed,
etc). This gives the system a simple audit trail.

Logging failures never crash the actual request - an audit log entry
is "nice to have", not something that should stop a customer from
placing an order.
"""

from database import get_cursor


def log_activity(user_type: str, user_id: int, action: str, details: str = "") -> None:
    """Record one activity log entry.

    user_type: "admin" or "customer"
    action: a short code like "LOGIN", "ORDER_PLACED", "PRODUCT_CREATED"
    details: a free-text human readable description
    """
    try:
        with get_cursor(commit=True) as cursor:
            cursor.execute(
                """
                INSERT INTO ActivityLogs (user_type, user_id, action, details)
                VALUES (%s, %s, %s, %s)
                """,
                (user_type, user_id, action, details),
            )
    except Exception:
        # Never let a logging problem break the actual feature.
        pass
