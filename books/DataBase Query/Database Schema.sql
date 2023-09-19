-- table creation schema

-- Create the Authors table
	CREATE TABLE Authors (
    author_id INT IDENTITY(1,1) PRIMARY KEY,
    author_name varchar(100)
);

-- Create the Publishers table
CREATE TABLE Publishers (
    publisher_id INT IDENTITY(1,1) PRIMARY KEY,
    publisher_name varchar(100),
published_date varchar(100)
);

-- Create the Books table
CREATE TABLE Books (
    book_id INT PRIMARY KEY,
    title varchar(100),
    author_id INT,
    publisher_id INT,
    description varchar(1000),
    FOREIGN KEY (author_id) REFERENCES Authors(author_id),
    FOREIGN KEY (publisher_id) REFERENCES Publishers(publisher_id)
);
