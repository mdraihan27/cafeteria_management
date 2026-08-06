"""
main.py
--------
Entry point for the Cafeteria Management System API.

To run this backend:
    1. Create/activate the virtual environment in Backend/venv
    2. pip install -r requirements.txt
    3. uvicorn main:app --reload --host 127.0.0.1 --port 8000

Then open http://127.0.0.1:8000/docs to see (and try out) every
endpoint in an interactive page - this is generated automatically by
FastAPI and is a great way to understand the API without reading any
frontend code.
"""

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

import config
from database import test_connection
from routes import auth, cart, categories, customers, dashboard, orders, products, reports

app = FastAPI(
    title="Cafeteria Management System API",
    description="Backend for the Cafeteria Management System desktop app.",
    version="1.0.0",
)

# The frontend is a desktop app (WinForms), not a website, so CORS
# doesn't really apply - but it's harmless to allow it, and it means
# you can also poke at this API from a browser-based tool if you want.
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Every route file defines its own APIRouter with its own URL prefix
# (e.g. "/products", "/orders"). Registering them here is what makes
# FastAPI actually serve them.
app.include_router(auth.router)
app.include_router(categories.router)
app.include_router(products.router)
app.include_router(customers.router)
app.include_router(cart.router)
app.include_router(orders.router)
app.include_router(dashboard.router)
app.include_router(reports.router)


@app.get("/", tags=["Health Check"])
def read_root():
    """A simple endpoint to confirm the server is running."""
    return {"message": "Cafeteria Management System API is running"}


@app.get("/health", tags=["Health Check"])
def health_check():
    """Confirms both the API and its database connection are working.
    The frontend's login screen calls this once at startup so it can
    show a clear "server not reachable" message instead of a
    confusing error."""
    db_ok = test_connection()
    return {
        "status": "ok" if db_ok else "degraded",
        "database_connected": db_ok,
    }


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("main:app", host=config.APP_HOST, port=config.APP_PORT, reload=True)
