-- UPDATE VALUES IN THE DATABASE WITH RESPECT TO BOOK ID
        
        UPDATE Books SET title = ISNULL(@Title, title),
        author_id = ISNULL(@Author_ID, author_id), 
        publisher_id = ISNULL(@Publisher_ID, publisher_id), 
        description = ISNULL(@Description, description) 
        WHERE book_id = @Id;