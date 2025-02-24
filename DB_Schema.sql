create database cec_aps;
use cec_aps;

-- CREATE TABLE customer (
--     id INT PRIMARY KEY AUTO_INCREMENT,
--     name VARCHAR(100) NOT NULL,
--     phone VARCHAR(20),
--     email VARCHAR(100),
--     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
--     updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
-- );

-- CREATE TABLE project (
--     id INT PRIMARY KEY AUTO_INCREMENT,
--     urn VARCHAR(255) NOT NULL,
--     name VARCHAR(100) NOT NULL,
--     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
--     updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
-- );

-- CREATE TABLE unit (
--     id INT PRIMARY KEY AUTO_INCREMENT,
--     project_id INT NOT NULL,
--     unit_number VARCHAR(50) NOT NULL,
--     floor INT,
--     owner_id INT,
--     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
--     updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
--     FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE CASCADE,
--     FOREIGN KEY (owner_id) REFERENCES customer(id) ON DELETE SET NULL
-- );

-- CREATE TABLE change_detail (
--     id INT PRIMARY KEY AUTO_INCREMENT,
--     db_id INT NOT NULL,
--     element_id INT NOT NULL,
--     status ENUM('Move', 'Delete', 'Add') NOT NULL
-- );

-- CREATE TABLE change_request (
--     id INT PRIMARY KEY AUTO_INCREMENT,
--     detail_id INT,
--     unit_id INT NOT NULL,
--     customer_id INT NOT NULL,
--     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
--     updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
--     FOREIGN KEY (detail_id) REFERENCES change_detail(id) ON DELETE SET NULL,
--     FOREIGN KEY (unit_id) REFERENCES unit(id) ON DELETE CASCADE,
--     FOREIGN KEY (customer_id) REFERENCES customer(id) ON DELETE CASCADE
-- );


Drop Table cec_aps.customer,cec_aps.unit,cec_aps.change_detail,cec_aps.project,cec_aps.change_request;

CREATE TABLE customer (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE project (
    id INT PRIMARY KEY AUTO_INCREMENT,
    urn VARCHAR(255) NOT NULL,
    name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE customer_project (
    customer_id INT NOT NULL,
    project_id INT NOT NULL,
    PRIMARY KEY (customer_id, project_id),
    FOREIGN KEY (customer_id) REFERENCES customer(id) ON DELETE CASCADE,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE CASCADE
);

CREATE TABLE unit (
    id INT PRIMARY KEY AUTO_INCREMENT,
    project_id INT NOT NULL,
    unit_number VARCHAR(50) NOT NULL,
    floor INT,
    customer_id INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE CASCADE,
    FOREIGN KEY (customer_id) REFERENCES customer(id) ON DELETE SET NULL
);

CREATE TABLE change_request (
    id INT PRIMARY KEY AUTO_INCREMENT,
    unit_id INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (unit_id) REFERENCES unit(id) ON DELETE CASCADE
);

CREATE TABLE change_detail (
    id INT PRIMARY KEY AUTO_INCREMENT,
    db_id INT NOT NULL,
    change_id INT,
    element_id INT NOT NULL,
    status ENUM('Move', 'Delete', 'Add') NOT NULL,
    vector VARCHAR(100),
    unit ENUM('MM', 'CM', 'M'),
    FOREIGN KEY (change_id) REFERENCES change_request(id) ON DELETE SET NULL
);

INSERT INTO customer (name, phone, email) VALUES
('Alice Johnson', '1234567890', 'alice@example.com'),
('Bob Smith', '2345678901', 'bob@example.com'),
('Charlie Brown', '3456789012', 'charlie@example.com'),
('David White', '4567890123', 'david@example.com'),
('Emma Green', '5678901234', 'emma@example.com'),
('Frank Black', '6789012345', 'frank@example.com'),
('Grace Hall', '7890123456', 'grace@example.com'),
('Henry Adams', '8901234567', 'henry@example.com'),
('Isla King', '9012345678', 'isla@example.com'),
('Jack Lee', '0123456789', 'jack@example.com');

INSERT INTO project (urn, name) VALUES
('URN123', 'Project Alpha'),
('URN456', 'Project Beta'),
('URN789', 'Project Gamma'),
('URN101', 'Project Delta'),
('URN202', 'Project Epsilon'),
('URN303', 'Project Zeta'),
('URN404', 'Project Eta'),
('URN505', 'Project Theta'),
('URN606', 'Project Iota'),
('URN707', 'Project Kappa');

INSERT INTO customer_project (customer_id, project_id) VALUES
(2, 1), (2, 2), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8), (9, 9), (10, 10);

SELECT * FROM customer;


SELECT * FROM customer_project;
CREATE TABLE customer_project (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT NOT NULL,
    project_id INT NOT NULL,
    FOREIGN KEY (customer_id) REFERENCES customer(id) ON DELETE CASCADE,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE CASCADE
);


DROP Table unit;
ALTER TABLE unit
ADD COLUMN customer_project_id INT;

-- 添加外鍵約束
ALTER TABLE unit
ADD CONSTRAINT fk_unit_customer_project
FOREIGN KEY (customer_project_id)
REFERENCES customer_project(id)
ON DELETE SET NULL;

SELECT * FROM customer_project;
DELETE FROM unit;
select * FROM unit;
INSERT INTO unit (project_id, unit_number, floor, customer_id) VALUES
(1, 'A101', 1, 1), (1, 'A102', 1, 2), (2, 'B201', 2, 3), (3, 'C301', 3, 4), (4, 'D401', 4, 5),
(5, 'E501', 5, 6), (6, 'F601', 6, 7), (7, 'G701', 7, 8), (8, 'H801', 8, 9), (9, 'I901', 9, 10);

INSERT INTO change_request (unit_id) VALUES
(1), (2), (3), (4), (5), (6), (7), (8), (9), (10);

INSERT INTO change_detail (db_id, change_id, element_id, status, vector, unit) VALUES
(1001, 1, 5001, 'Move', '10,20,30', 'CM'), (1002, 2, 5002, 'Delete', '0,0,0', 'MM'), (1003, 3, 5003, 'Add', '5,5,5', 'M'),
(1004, 4, 5004, 'Move', '15,25,35', 'CM'), (1005, 5, 5005, 'Delete', '1,1,1', 'MM'), (1006, 6, 5006, 'Add', '6,6,6', 'M'),
(1007, 7, 5007, 'Move', '20,30,40', 'CM'), (1008, 8, 5008, 'Delete', '2,2,2', 'MM'), (1009, 9, 5009, 'Add', '7,7,7', 'M'),
(1010, 10, 5010, 'Move', '25,35,45', 'CM');

SELECT * FROM customerproject
LEFT JOIN customer_project AS cp
ON customer.id = cp.customer_id;

SELECT * FROM project;
DESCRIBE project;

SELECT * FROM unit
LEFT JOIN customer
ON unit.customer_id = customer.id;

SELECT * FROM unit WHERE project_id = 1 AND customer_id=2;
describe unit;

SELECT * FROM customer;





ALTER TABLE change_detail DROP COLUMN change_id;

ALTER TABLE change_detail ADD COLUMN change_id INT NULL;

UPDATE change_detail
SET unit_id = 3;
SELECT * FROM change_detail;
SELECT * FROM unit;

ALTER TABLE change_detail ADD COLUMN unit_id INT NOT NULL;

-- ALTER TABLE change_detail ADD CONSTRAINT fk_change_detail_unit
-- FOREIGN KEY (unit_id) REFERENCES unit(id) ON DELETE CASCADE;

ALTER TABLE change_detail ADD CONSTRAINT
FOREIGN KEY (unit_id) REFERENCES unit(id) ON DELETE CASCADE;

describe change_detail;
SELECT * FROM cec_aps.change_detail;

SHOW CREATE TABLE unit;
SELECT * FROM unit;

SELECT * FROM unit u
LEFT JOIN customer_project cp ON u.customer_id = cp.customer_id AND u.project_id = cp.project_id;

SELECT * FROM customer_project;

UPDATE unit
SET unit.customer_project_id = (
	SELECT cp.id
    FROM customer_project cp
    WHERE cp.customer_id = unit.customer_id
    AND cp.project_id = unit.project_id
);


UPDATE unit u
JOIN customer_project cp ON u.customer_id = cp.customer_id AND u.project_id = cp.project_id
SET u.customer_project_id = cp.id;

SELECT * FROM change_detail;
SELECT * FROM change_request;
select * FROM project;
SELECT * FROM customer;


SELECT * FROM unit;
-- 移除欄位
ALTER TABLE unit
DROP COLUMN customer_id;

ALTER TABLE unit
DROP COLUMN project_id;


SELECT * FROM unit;
SELECT * FROM change_detail LEFT JOIN unit
ON change_detail.unit_id = unit.id;

SELECT * FROM customer_project;
SELECT * FROM unit 
WHERE customer_project_id IN (SELECT cp.id FROM customer_project cp WHERE cp.customer_id = 4 AND cp.project_id =4);
UPDATE unit u SET u.customer_project_id = 16
WHERE u.id = 17;
