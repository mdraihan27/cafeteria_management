-- =====================================================================
-- Cafeteria Management System - Database Schema
-- =====================================================================
-- This file creates the database and all tables used by the system.
--
-- HOW TO RUN THIS FILE (beginner friendly):
--   1. Open a terminal in the Database/ folder.
--   2. Run:  mysql -u root -p < cafeteria.sql
--      (or open this file in MySQL Workbench and click "Execute".)
--   3. Then run sample_data.sql the same way to add example data.
--
-- The scripts/ folder in the project root also has a one-click
-- setup script that does all of this automatically.
-- =====================================================================

DROP DATABASE IF EXISTS cafeteria_db;
CREATE DATABASE cafeteria_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE cafeteria_db;

-- ---------------------------------------------------------------------
-- Categories
-- Every product belongs to exactly one category.
-- ---------------------------------------------------------------------
CREATE TABLE Categories (
    category_id     INT AUTO_INCREMENT PRIMARY KEY,
    category_name   VARCHAR(100) NOT NULL UNIQUE,
    description     VARCHAR(255) NULL,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ---------------------------------------------------------------------
-- Admins
-- Staff who manage the cafeteria (products, orders, reports).
-- ---------------------------------------------------------------------
CREATE TABLE Admins (
    admin_id        INT AUTO_INCREMENT PRIMARY KEY,
    full_name       VARCHAR(100) NOT NULL,
    username        VARCHAR(50)  NOT NULL UNIQUE,
    email           VARCHAR(100) NOT NULL UNIQUE,
    password_hash   VARCHAR(255) NOT NULL,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ---------------------------------------------------------------------
-- Customers
-- People who browse the menu and place orders.
-- ---------------------------------------------------------------------
CREATE TABLE Customers (
    customer_id     INT AUTO_INCREMENT PRIMARY KEY,
    full_name       VARCHAR(100) NOT NULL,
    email           VARCHAR(100) NOT NULL UNIQUE,
    password_hash   VARCHAR(255) NOT NULL,
    phone           VARCHAR(20)  NULL,
    address         VARCHAR(255) NULL,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ---------------------------------------------------------------------
-- Products
-- Rule: product names must be unique.
-- Rule: stock quantity can never become negative (enforced with CHECK).
-- Note: "deleting" a product from the Admin app is a SOFT delete
-- (is_active = 0). This keeps old orders readable even after a
-- product is removed from the menu, per the business rules.
-- ---------------------------------------------------------------------
CREATE TABLE Products (
    product_id      INT AUTO_INCREMENT PRIMARY KEY,
    product_name    VARCHAR(150) NOT NULL UNIQUE,
    description     VARCHAR(500) NULL,
    price           DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    stock_quantity  INT NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    category_id     INT NOT NULL,
    is_active       TINYINT(1) NOT NULL DEFAULT 1,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_products_category FOREIGN KEY (category_id)
        REFERENCES Categories(category_id) ON DELETE RESTRICT
);

CREATE INDEX idx_products_category ON Products(category_id);

-- ---------------------------------------------------------------------
-- Cart
-- Every customer has at most one cart. It is created automatically
-- the first time they add a product to it.
-- ---------------------------------------------------------------------
CREATE TABLE Cart (
    cart_id         INT AUTO_INCREMENT PRIMARY KEY,
    customer_id     INT NOT NULL UNIQUE,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_cart_customer FOREIGN KEY (customer_id)
        REFERENCES Customers(customer_id) ON DELETE CASCADE
);

-- ---------------------------------------------------------------------
-- CartItems
-- ---------------------------------------------------------------------
CREATE TABLE CartItems (
    cart_item_id    INT AUTO_INCREMENT PRIMARY KEY,
    cart_id         INT NOT NULL,
    product_id      INT NOT NULL,
    quantity        INT NOT NULL CHECK (quantity > 0),
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_cartitems_cart FOREIGN KEY (cart_id)
        REFERENCES Cart(cart_id) ON DELETE CASCADE,
    CONSTRAINT fk_cartitems_product FOREIGN KEY (product_id)
        REFERENCES Products(product_id) ON DELETE CASCADE,
    CONSTRAINT uq_cart_product UNIQUE (cart_id, product_id)
);

-- ---------------------------------------------------------------------
-- Orders
-- Rule: customers can only cancel orders while they are "pending".
-- Rule: every order must contain at least one product (enforced in
--       the API layer, since a table-level check can't easily count
--       related rows).
-- ---------------------------------------------------------------------
CREATE TABLE Orders (
    order_id        INT AUTO_INCREMENT PRIMARY KEY,
    customer_id     INT NOT NULL,
    order_date      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status          ENUM('pending', 'completed', 'cancelled') NOT NULL DEFAULT 'pending',
    total_amount    DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_orders_customer FOREIGN KEY (customer_id)
        REFERENCES Customers(customer_id) ON DELETE RESTRICT
);

CREATE INDEX idx_orders_customer ON Orders(customer_id);
CREATE INDEX idx_orders_status ON Orders(status);
CREATE INDEX idx_orders_date ON Orders(order_date);

-- ---------------------------------------------------------------------
-- OrderItems
-- Rule: order history must survive even if the product is later
-- deleted. We store a snapshot of the product's name and price at
-- the time of purchase, and product_id is set to NULL instead of
-- the row being deleted if the product ever goes away.
-- ---------------------------------------------------------------------
CREATE TABLE OrderItems (
    order_item_id   INT AUTO_INCREMENT PRIMARY KEY,
    order_id        INT NOT NULL,
    product_id      INT NULL,
    product_name    VARCHAR(150) NOT NULL,
    unit_price      DECIMAL(10,2) NOT NULL,
    quantity        INT NOT NULL CHECK (quantity > 0),
    subtotal        DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_orderitems_order FOREIGN KEY (order_id)
        REFERENCES Orders(order_id) ON DELETE CASCADE,
    CONSTRAINT fk_orderitems_product FOREIGN KEY (product_id)
        REFERENCES Products(product_id) ON DELETE SET NULL
);

CREATE INDEX idx_orderitems_order ON OrderItems(order_id);
CREATE INDEX idx_orderitems_product ON OrderItems(product_id);

-- ---------------------------------------------------------------------
-- Payments
-- Rule: every completed order must have a payment record.
-- ---------------------------------------------------------------------
CREATE TABLE Payments (
    payment_id      INT AUTO_INCREMENT PRIMARY KEY,
    order_id        INT NOT NULL UNIQUE,
    amount          DECIMAL(10,2) NOT NULL,
    payment_method  ENUM('cash', 'card', 'mobile_banking') NOT NULL DEFAULT 'cash',
    payment_status  ENUM('paid', 'refunded') NOT NULL DEFAULT 'paid',
    payment_date    TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_payments_order FOREIGN KEY (order_id)
        REFERENCES Orders(order_id) ON DELETE CASCADE
);

-- ---------------------------------------------------------------------
-- ActivityLogs
-- A simple audit trail of important actions (logins, orders, etc).
-- user_type + user_id together identify who did the action, since
-- admins and customers live in two different tables.
-- ---------------------------------------------------------------------
CREATE TABLE ActivityLogs (
    log_id          INT AUTO_INCREMENT PRIMARY KEY,
    user_type       ENUM('admin', 'customer') NOT NULL,
    user_id         INT NOT NULL,
    action          VARCHAR(100) NOT NULL,
    details         VARCHAR(500) NULL,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_activitylogs_user ON ActivityLogs(user_type, user_id);
CREATE INDEX idx_activitylogs_created ON ActivityLogs(created_at);

-- ---------------------------------------------------------------------
-- Dedicated application database user (best practice: the backend
-- should not connect as "root"). Change this password before using
-- this project anywhere other than your own machine.
-- ---------------------------------------------------------------------
CREATE USER IF NOT EXISTS 'cafeteria_app'@'localhost' IDENTIFIED BY 'CafeteriaApp@123';
GRANT SELECT, INSERT, UPDATE, DELETE ON cafeteria_db.* TO 'cafeteria_app'@'localhost';
FLUSH PRIVILEGES;
