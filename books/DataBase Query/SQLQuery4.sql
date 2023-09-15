/* Query to Retrieve all book from database /*

SELECT b.book_id, b.title, a.author_name, p.publisher_name, p.published_date, b.description FROM Books b INNER JOIN Authors a ON b.author_id = a.author_id INNER JOIN Publishers p ON b.publisher_id = p.publisher_id WHERE b.book_id = @BookId;