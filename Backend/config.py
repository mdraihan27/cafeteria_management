"""
config.py
---------
Central place for all configuration values used by the backend.

Everything here is read from environment variables (loaded from the
.env file in this folder). Keeping configuration in one file makes it
easy to find every "knob" the app has, and keeps secrets (passwords,
JWT keys) out of the actual source code.

If you are new to this project: open the ".env" file next to this
one to see the actual values used on your machine.
"""

import os
from dotenv import load_dotenv

# Load variables from the .env file into the process environment.
load_dotenv()


def _get_env(name: str, default: str | None = None) -> str:
    """Read an environment variable, falling back to a default value.

    Raises a clear error if a required value is missing, instead of
    letting the app start in a broken state.
    """
    value = os.getenv(name, default)
    if value is None:
        raise RuntimeError(
            f"Missing required environment variable '{name}'. "
            f"Check your Backend/.env file."
        )
    return value


# --- Database connection settings -----------------------------------
DB_HOST = _get_env("DB_HOST", "127.0.0.1")
DB_PORT = int(_get_env("DB_PORT", "3306"))
DB_USER = _get_env("DB_USER", "cafeteria_app")
DB_PASSWORD = _get_env("DB_PASSWORD", "CafeteriaApp@123")
DB_NAME = _get_env("DB_NAME", "cafeteria_db")

# --- JWT / authentication settings -----------------------------------
JWT_SECRET_KEY = _get_env("JWT_SECRET_KEY", "change-this-secret-key")
JWT_ALGORITHM = "HS256"
JWT_EXPIRE_MINUTES = int(_get_env("JWT_EXPIRE_MINUTES", "480"))  # 8 hours

# --- General app settings --------------------------------------------
APP_HOST = _get_env("APP_HOST", "127.0.0.1")
APP_PORT = int(_get_env("APP_PORT", "8000"))

# Products with stock at or below this number are considered "low stock"
# on the admin dashboard and inventory report.
LOW_STOCK_THRESHOLD = int(_get_env("LOW_STOCK_THRESHOLD", "10"))
