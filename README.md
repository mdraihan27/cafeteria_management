# Cafeteria Management System

A desktop application for running a cafeteria: customers browse a menu, add
items to a cart, and place orders; administrators manage products,
categories, orders, and view sales/inventory reports.

This is a **monorepo** with three parts that all live side by side:

```
restaurant-management-system/
├── Frontend/     C# WinForms desktop app (what the user sees and clicks)
├── Backend/      Python FastAPI server (all the business logic + database access)
├── Database/     MySQL schema + sample data
└── scripts/      One-click setup/run scripts for Windows
```

The Frontend never talks to the database directly - it only ever calls the
Backend's HTTP API, and the Backend is the only thing that talks to MySQL.
That separation is what makes each of the three folders understandable on
its own.

---

## 1. What you need installed

| Tool | Why | Checked automatically? |
|---|---|---|
| **.NET SDK 8 or newer** | Builds/runs the Frontend (WinForms) | `scripts/first-time-setup.bat` will tell you if it's missing |
| **Python 3.12** | Runs the Backend | same |
| **MySQL Server 8.x** | The database | same |

If you're missing any of these, install them first:
- .NET SDK: https://dotnet.microsoft.com/download
- Python: https://www.python.org/downloads/ (make sure "Add to PATH" is checked,
  or just make sure the `py` launcher works)
- MySQL Server: https://dev.mysql.com/downloads/mysql/

This project connects to MySQL on the **default port 3306**, using whatever
MySQL server is already installed/running on your machine (Windows service
name `MySQL80` if you installed it with the standard installer). The app
itself never connects as `root` - `Database/cafeteria.sql` creates a
dedicated `cafeteria_app` user with its own password, and that's what
`Backend/.env` actually points at. Root credentials are only needed once,
by hand, to load the schema (see the next section).

---

## 2. First-time setup

Run this once, from the `scripts` folder:

```
scripts\first-time-setup.bat
```

This will:
1. Create a Python virtual environment in `Backend/venv` and install its
   dependencies.
2. Ask for your MySQL **root** password (only used right then, to run the
   two `.sql` files below - not saved anywhere), and load the schema
   (`Database/cafeteria.sql`) and demo data (`Database/sample_data.sql`).
3. Restore and build the Frontend project.

It's safe to run again later - anything already done gets skipped (re-running
step 2 resets the database back to the sample data, if you want that).

> MySQL itself needs to already be installed and running before this script
> can load the schema - see `scripts/start-database.bat` if you're not sure
> whether it's running.

---

## 3. Running the app

MySQL runs as a Windows service, so it's typically already running (or set
to start automatically) - `scripts\start-database.bat` just double-checks
that and tells you how to start it if not (starting a Windows service needs
an elevated terminal, so that one step can't be fully automated).

Then open **two** terminal windows (or just double-click these two files):

```
scripts\start-backend.bat      (leave running)
scripts\start-frontend.bat     (this is the actual app window)
```

The backend's interactive API docs are at http://127.0.0.1:8000/docs - a
good place to see every endpoint and try them out directly, independent of
the desktop app.

### Running it by hand, from a terminal (no .bat files)

The scripts above are just conveniences - here's exactly what each one
does, if you'd rather run (or script) it yourself. The database step is
one-time setup; backend and frontend each stay running in the foreground,
so give those two their own terminal windows.

**Database (one-time setup, not a process to leave running):**

```powershell
cd Database
# MySQL server should already be running (it's a Windows service - check
# with `Get-Service MySQL80`, start it from an Administrator terminal with
# `net start MySQL80` if it isn't). Then load the schema and demo data:
mysql -u root -p < cafeteria.sql
mysql -u root -p cafeteria_db < sample_data.sql
```

**Terminal 1 - backend:**

```powershell
cd Backend
py -3.12 -m venv venv          # first time only
.\venv\Scripts\Activate.ps1
pip install -r requirements.txt   # first time only
copy .env.example .env            # first time only
uvicorn main:app --host 127.0.0.1 --port 8000 --reload
```

**Terminal 2 - frontend:**

```powershell
cd Frontend
dotnet build     # first time, or after any code changes
dotnet run        # builds (if needed) and launches the desktop app
```

You should now have MySQL, the API, and the desktop app all running. Log
in with the demo credentials below.

### Demo login credentials

| Role | Username / Email | Password |
|---|---|---|
| Admin | `admin` | `Admin@123` |
| Customer | `john@example.com` | `Customer@123` |
| Customer | `jane@example.com` | `Customer@123` |
| Customer | `michael@example.com` | `Customer@123` |

(Or click "New here? Create an account" on the customer login screen to
register your own.)

---

## 4. Project layout, in detail

### Frontend/ (C#, WinForms, .NET)

```
Frontend/
├── Program.cs          Entry point - just opens LoginForm
├── Forms/               Every screen in the app (one class per screen)
│   ├── BaseForm.cs      Every window inherits from this - applies the
│   │                    app's look automatically (see Utilities/Theme.cs)
│   ├── LoginForm.cs      "Are you an Admin or a Customer?"
│   ├── AppMessageBox.cs Our own popup, used instead of MessageBox.Show
│   ├── OrderDetailForm.cs  Shared by both Admin and Customer order screens
│   ├── Admin/           Admin-only screens (Dashboard, Products, ...)
│   └── Customer/        Customer-only screens (Menu, Cart, ...)
├── Models/              Plain C# classes matching the backend's JSON shapes
├── Services/            One class per backend area (ProductService,
│                        OrderService, ...) - all HTTP calls go through
│                        Services/ApiClient.cs
└── Utilities/           Shared helpers: Theme.cs (colors/fonts),
                         SessionManager.cs (who's logged in),
                         UiHelpers.cs (control factory methods)
```

**How screens are organized:** after logging in, `AdminMainForm` (or
`CustomerMainForm`) shows a left-hand navigation panel and swaps whichever
screen you clicked into a content area on the right. Each "screen" is just
a normal `Form` - see `Utilities/FormEmbedder.cs` for the (small) trick that
lets a Form be displayed inside a Panel like that.

**Look and feel:** the whole app uses exactly two colors - white
backgrounds and black text, with buttons inverted (black background, white
text) so they stand out as clickable. Nothing is rounded anywhere. This is
all defined in one place, `Utilities/Theme.cs`, and applied automatically
by `BaseForm` - individual screens never set colors themselves.

### Backend/ (Python, FastAPI)

```
Backend/
├── main.py           Creates the FastAPI app and registers every route file
├── config.py         All settings, read from .env
├── database.py       The only file that imports mysql.connector directly
├── routes/           One file per feature area (auth, products, orders, ...)
├── models/           Pydantic schemas (request/response shapes) per feature
├── utils/            security.py (bcrypt + JWT), dependencies.py (login
│                     checks), activity_logger.py
└── requirements.txt
```

There is **no ORM** - every query in `routes/*.py` is plain SQL through
`mysql-connector-python`, on purpose (per the project spec). `database.py`
provides one small `get_cursor()` helper that every route uses, so the
connection-pooling and transaction/rollback logic only has to be written
once.

Passwords are hashed with **bcrypt**; logins hand back a **JWT** that the
frontend attaches to every subsequent request (`Authorization: Bearer ...`).
See `utils/security.py` and `utils/dependencies.py`.

Try it directly (no frontend needed) at http://127.0.0.1:8000/docs once
`start-backend.bat` is running.

### Database/

```
Database/
├── cafeteria.sql       Full schema: 10 tables, foreign keys, CHECK
│                       constraints for the business rules that can be
│                       enforced at the database level (e.g. stock can
│                       never go negative)
└── sample_data.sql     Demo categories/products/customers/orders
```

These two files are the actual database - your MySQL server just runs them
and stores the resulting data itself (in its own data directory, wherever
your MySQL install keeps that; not part of this repo).

---

## 5. Business rules and where they live

| Rule | Enforced in |
|---|---|
| Every product belongs to one category | `Products.category_id` foreign key (`cafeteria.sql`) |
| Product names must be unique | Unique constraint + a friendly check in `routes/products.py` |
| Stock quantity can never go negative | `CHECK` constraint in `cafeteria.sql` + application-level check in `routes/orders.py` |
| Products with zero stock cannot be ordered | `routes/cart.py` and `routes/orders.py` |
| Customers can only cancel pending orders | `routes/orders.py` (`cancel_my_order`) |
| Every completed order must have a payment record | `routes/orders.py` (`update_order_status`) creates the Payment in the same request that marks an order completed |
| Every order must contain at least one product | `routes/orders.py` (`checkout` rejects an empty cart) |
| Administrators can manage all orders | `routes/orders.py` admin endpoints (view/complete/cancel any order) |
| Order history survives a deleted product | Products are **soft-deleted** (`is_active = 0`, never actually removed), and `OrderItems` also stores a snapshot of the product's name/price at the time of the order either way |

---

## 6. A note on the "no ORM" / hand-written SQL style

Every route file follows the same shape: look something up, check the
business rule, run the SQL, log the activity. It's intentionally repetitive
rather than clever, so that opening any single `routes/*.py` file explains
itself without needing to understand the other nine.
