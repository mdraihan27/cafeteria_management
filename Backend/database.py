"""
database.py
-----------
Everything related to talking to MySQL lives here. There is no ORM in
this project (by design, per the project spec) - we write plain SQL
using mysql-connector-python and get plain Python dictionaries back.

The rest of the app never imports mysql.connector directly; it always
goes through `get_cursor()` below. That keeps all the database
plumbing in one place, which makes the code easier to follow.
"""

from contextlib import contextmanager

import mysql.connector
from mysql.connector import pooling

import config

# A connection pool is created once when the app starts, and reused
# for every request. This is much faster than opening a brand new
# database connection every time.
_pool = pooling.MySQLConnectionPool(
    pool_name="cafeteria_pool",
    pool_size=5,
    host=config.DB_HOST,
    port=config.DB_PORT,
    user=config.DB_USER,
    password=config.DB_PASSWORD,
    database=config.DB_NAME,
)


@contextmanager
def get_cursor(commit: bool = False):
    """Give calling code a ready-to-use database cursor.

    Usage example:

        with get_cursor() as cursor:
            cursor.execute("SELECT * FROM Products")
            rows = cursor.fetchall()

    Pass commit=True for INSERT/UPDATE/DELETE statements so the
    change is actually saved:

        with get_cursor(commit=True) as cursor:
            cursor.execute("UPDATE Products SET stock_quantity = %s WHERE product_id = %s", (5, 1))

    Any exception automatically rolls back the transaction so a
    failed request never leaves half-finished changes behind.
    """
    connection = _pool.get_connection()
    # dictionary=True makes each row come back as {"column_name": value, ...}
    # instead of a plain tuple, which matches our Pydantic model field names.
    cursor = connection.cursor(dictionary=True)
    try:
        yield cursor
        if commit:
            connection.commit()
    except Exception:
        connection.rollback()
        raise
    finally:
        cursor.close()
        connection.close()


def test_connection() -> bool:
    """Quick check used at startup to confirm the database is reachable."""
    try:
        with get_cursor() as cursor:
            cursor.execute("SELECT 1")
            cursor.fetchall()
        return True
    except mysql.connector.Error:
        return False
