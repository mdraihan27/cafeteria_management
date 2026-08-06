-- =====================================================================
-- Cafeteria Management System - Sample Data
-- =====================================================================
-- Run this AFTER cafeteria.sql. It fills the database with example
-- categories, products, an admin account, a few customers, and some
-- past orders so the Dashboard and Reports screens have data to show
-- the first time you run the app.
--
--   mysql -u root -p cafeteria_db < sample_data.sql
--
-- Demo login credentials (see README.md for the full list):
--   Admin login:    username "admin"            password "Admin@123"
--   Customer login: email "john@example.com"    password "Customer@123"
-- =====================================================================

USE cafeteria_db;

-- ---------------------------------------------------------------------
-- Admin account
-- Password hash below is bcrypt("Admin@123")
-- ---------------------------------------------------------------------
INSERT INTO Admins (full_name, username, email, password_hash) VALUES
('Raihan Ahmed', 'admin', 'admin@cafeteria.com', '$2b$12$VRzTog0XOJe8pQlnsisld.URf68iQ8eH/Z13JVsW01ACiEdZbzqH6');

-- ---------------------------------------------------------------------
-- Customers
-- Password hash below (for all 3) is bcrypt("Customer@123")
-- ---------------------------------------------------------------------
INSERT INTO Customers (full_name, email, password_hash, phone, address) VALUES
('John Doe',      'john@example.com',    '$2b$12$PSYsvYohNMcmNWkvAnL0JOymhYaohUMiIrI7XBoR3T8RrpufEZ/v6', '01700000001', '12 Gulshan Avenue, Dhaka'),
('Jane Smith',    'jane@example.com',    '$2b$12$PSYsvYohNMcmNWkvAnL0JOymhYaohUMiIrI7XBoR3T8RrpufEZ/v6', '01700000002', '45 Banani Road, Dhaka'),
('Michael Lee',   'michael@example.com', '$2b$12$PSYsvYohNMcmNWkvAnL0JOymhYaohUMiIrI7XBoR3T8RrpufEZ/v6', '01700000003', '9 Dhanmondi Lane, Dhaka');

-- ---------------------------------------------------------------------
-- Categories
-- ---------------------------------------------------------------------
INSERT INTO Categories (category_name, description) VALUES
('Beverages',    'Hot and cold drinks'),
('Appetizers',   'Starters and small bites'),
('Main Course',  'Full meals'),
('Desserts',     'Sweet treats'),
('Snacks',       'Quick light bites');

-- ---------------------------------------------------------------------
-- Products (category_id 1=Beverages, 2=Appetizers, 3=Main Course,
-- 4=Desserts, 5=Snacks)
-- A couple of items are deliberately low stock or zero stock so you
-- can see the "Low Stock" dashboard widget and the "can't order
-- out-of-stock items" rule working right away.
-- ---------------------------------------------------------------------
INSERT INTO Products (product_name, description, price, stock_quantity, category_id) VALUES
('Iced Coffee',          'Chilled coffee served over ice',            3.50, 40, 1),
('Hot Tea',               'Freshly brewed black tea',                  1.50, 60, 1),
('Orange Juice',          'Freshly squeezed orange juice',             2.75, 25, 1),
('Mineral Water',         '500ml bottled water',                       1.00, 100, 1),
('Chicken Wings',         'Crispy fried chicken wings (6 pcs)',        5.50, 20, 2),
('Spring Rolls',          'Vegetable spring rolls (4 pcs)',            4.00, 15, 2),
('Garlic Bread',          'Toasted garlic bread with herbs',           3.00, 8,  2),
('Grilled Chicken Meal',  'Grilled chicken with rice and salad',       8.50, 18, 3),
('Beef Burger',           'Beef patty burger with fries',              7.25, 22, 3),
('Vegetable Pasta',       'Pasta tossed with seasonal vegetables',     6.75, 12, 3),
('Fried Rice',            'Chicken fried rice',                        6.00, 30, 3),
('Chocolate Cake',        'Rich chocolate layer cake',                 4.25, 10, 4),
('Ice Cream Sundae',      'Vanilla ice cream with toppings',           3.75, 5,  4),
('Fruit Salad',           'Mixed seasonal fruit salad',                3.25, 0,  4),
('Potato Chips',          'Salted potato chips',                       1.75, 50, 5),
('Chicken Samosa',        'Fried samosa with chicken filling (3 pcs)', 2.50, 6,  5),
('Sandwich',              'Grilled chicken and cheese sandwich',       4.50, 16, 5),
('Cheese Nachos',         'Tortilla chips with melted cheese',         4.75, 0,  5);

-- ---------------------------------------------------------------------
-- Sample Orders + OrderItems + Payments
-- Spread across 2025 and 2026 so the Monthly/Yearly Sales Reports and
-- the Product Sales Report have meaningful data to display.
-- Product prices are copied into OrderItems as a snapshot, matching
-- the "history survives even if the product changes" business rule.
-- ---------------------------------------------------------------------

-- 2025 orders (completed, for yearly comparison)
INSERT INTO Orders (customer_id, order_date, status, total_amount) VALUES
(1, '2025-03-14 12:30:00', 'completed', 16.00),
(2, '2025-06-02 18:10:00', 'completed', 21.25),
(3, '2025-09-21 13:05:00', 'completed', 11.50),
(1, '2025-11-30 19:45:00', 'completed', 26.25);

INSERT INTO OrderItems (order_id, product_id, product_name, unit_price, quantity, subtotal) VALUES
(1, 1, 'Iced Coffee', 3.50, 1, 3.50), (1, 9, 'Beef Burger', 7.25, 1, 7.25), (1, 15, 'Potato Chips', 1.75, 3, 5.25),
(2, 8, 'Grilled Chicken Meal', 8.50, 2, 17.00), (2, 12, 'Chocolate Cake', 4.25, 1, 4.25),
(3, 11, 'Fried Rice', 6.00, 1, 6.00), (3, 2, 'Hot Tea', 1.50, 1, 1.50), (3, 6, 'Spring Rolls', 4.00, 1, 4.00),
(4, 10, 'Vegetable Pasta', 6.75, 2, 13.50), (4, 5, 'Chicken Wings', 5.50, 1, 5.50), (4, 3, 'Orange Juice', 2.75, 1, 2.75), (4, 17, 'Sandwich', 4.50, 1, 4.50);

INSERT INTO Payments (order_id, amount, payment_method, payment_status, payment_date) VALUES
(1, 16.00, 'cash', 'paid', '2025-03-14 12:32:00'),
(2, 21.25, 'card', 'paid', '2025-06-02 18:12:00'),
(3, 11.50, 'cash', 'paid', '2025-09-21 13:07:00'),
(4, 26.25, 'mobile_banking', 'paid', '2025-11-30 19:47:00');

-- 2026 orders, Jan through Aug (current month), mostly completed with
-- two pending and one cancelled so those statuses can be seen too.
INSERT INTO Orders (customer_id, order_date, status, total_amount) VALUES
(2, '2026-01-10 09:15:00', 'completed', 11.25),
(1, '2026-02-18 14:20:00', 'completed', 18.00),
(3, '2026-03-05 11:00:00', 'completed', 10.00),
(2, '2026-04-22 17:40:00', 'completed', 24.25),
(1, '2026-05-09 12:15:00', 'completed', 16.25),
(3, '2026-06-14 19:30:00', 'completed', 20.50),
(2, '2026-07-01 13:45:00', 'completed', 11.50),
(1, '2026-08-02 10:05:00', 'completed', 15.75),
(3, '2026-08-05 20:00:00', 'pending',   13.00),
(2, '2026-08-06 08:30:00', 'pending',    9.00),
(1, '2026-07-20 16:00:00', 'cancelled', 11.00);

INSERT INTO OrderItems (order_id, product_id, product_name, unit_price, quantity, subtotal) VALUES
(5, 4, 'Mineral Water', 1.00, 2, 2.00), (5, 16, 'Chicken Samosa', 2.50, 2, 5.00), (5, 2, 'Hot Tea', 1.50, 1, 1.50), (5, 3, 'Orange Juice', 2.75, 1, 2.75),
(6, 9, 'Beef Burger', 7.25, 1, 7.25), (6, 13, 'Ice Cream Sundae', 3.75, 1, 3.75), (6, 1, 'Iced Coffee', 3.50, 2, 7.00),
(7, 11, 'Fried Rice', 6.00, 1, 6.00), (7, 6, 'Spring Rolls', 4.00, 1, 4.00),
(8, 8, 'Grilled Chicken Meal', 8.50, 2, 17.00), (8, 12, 'Chocolate Cake', 4.25, 1, 4.25), (8, 4, 'Mineral Water', 1.00, 3, 3.00),
(9, 10, 'Vegetable Pasta', 6.75, 2, 13.50), (9, 3, 'Orange Juice', 2.75, 1, 2.75),
(10, 5, 'Chicken Wings', 5.50, 2, 11.00), (10, 15, 'Potato Chips', 1.75, 2, 3.50), (10, 2, 'Hot Tea', 1.50, 4, 6.00),
(11, 17, 'Sandwich', 4.50, 1, 4.50), (11, 1, 'Iced Coffee', 3.50, 2, 7.00),
(12, 8, 'Grilled Chicken Meal', 8.50, 1, 8.50), (12, 9, 'Beef Burger', 7.25, 1, 7.25),
(13, 11, 'Fried Rice', 6.00, 1, 6.00), (13, 16, 'Chicken Samosa', 2.50, 2, 5.00), (13, 4, 'Mineral Water', 1.00, 2, 2.00),
(14, 3, 'Orange Juice', 2.75, 1, 2.75), (14, 15, 'Potato Chips', 1.75, 1, 1.75), (14, 17, 'Sandwich', 4.50, 1, 4.50),
(15, 10, 'Vegetable Pasta', 6.75, 1, 6.75), (15, 12, 'Chocolate Cake', 4.25, 1, 4.25);

INSERT INTO Payments (order_id, amount, payment_method, payment_status, payment_date) VALUES
(5, 11.25, 'cash', 'paid', '2026-01-10 09:17:00'),
(6, 18.00, 'card', 'paid', '2026-02-18 14:22:00'),
(7, 10.00, 'cash', 'paid', '2026-03-05 11:02:00'),
(8, 24.25, 'mobile_banking', 'paid', '2026-04-22 17:42:00'),
(9, 16.25, 'cash', 'paid', '2026-05-09 12:17:00'),
(10, 20.50, 'card', 'paid', '2026-06-14 19:32:00'),
(11, 11.50, 'cash', 'paid', '2026-07-01 13:47:00'),
(12, 15.75, 'mobile_banking', 'paid', '2026-08-02 10:07:00');
-- Note: orders 13 & 14 (2026-08-05/06) are "pending" and order 15 is
-- "cancelled" - per business rules, those do NOT get payment rows
-- until/unless an admin marks them completed.

-- ---------------------------------------------------------------------
-- Sample activity log entries
-- ---------------------------------------------------------------------
INSERT INTO ActivityLogs (user_type, user_id, action, details) VALUES
('admin', 1, 'LOGIN', 'Admin logged in'),
('customer', 1, 'REGISTER', 'New customer account created'),
('customer', 1, 'ORDER_PLACED', 'Order #1 placed');
