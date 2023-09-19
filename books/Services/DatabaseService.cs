using books.Interfaces;
using books.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace books.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IConfiguration _configuration;
 
        public DatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
     
        }

        /// <summary>
        /// RetrieveBookFromDatabase this method is used to retrieve the books from the database using bookId as the parameter
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public Book RetrieveBookFromDatabase(int bookId)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = "SELECT b.book_id, b.title, a.author_name, p.publisher_name, p.published_date, b.description" +
                                       " FROM Books b" +
                                       " INNER JOIN Authors a ON b.author_id = a.author_id" +
                                       " INNER JOIN Publishers p ON b.publisher_id = p.publisher_id" +
                                       " WHERE b.book_id = @BookId";
                    SqlCommand command = new SqlCommand(selectSql, connection);
                    command.Parameters.AddWithValue("@BookId", bookId);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        var book = new Book
                        {
                            publisher_id = reader.GetInt32(0),
                            title = reader.GetString(1),
                            author_name = reader.GetString(2),
                            publisher_name = reader.GetString(3),
                            published_date = reader.GetString(4),
                            description = reader.GetString(5)
                        };

                        return book;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return null;
        }


        /// <summary>
        /// DeleteBookByIdFromDatabase method deletes the repecting bookId provided as a parameter
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public bool DeleteBookByIdFromDatabase(int bookId)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string deleteSql = "DELETE FROM Books WHERE book_id = @BookId";
                    SqlCommand command = new SqlCommand(deleteSql, connection);
                    command.Parameters.AddWithValue("@BookId", bookId);

                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// RetrieveBooksFromDatabase() method is used to retrieve the values from the database
        /// </summary>
        /// <returns></returns>
        public List<Book> RetrieveBooksFromDatabase()
        {
            List<Book> books = new List<Book>();

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = " SELECT b.book_id, b.title, a.author_name, p.publisher_name, p.published_date, b.description\r\n FROM Books b\r\n INNER JOIN Authors a ON b.author_id = a.author_id\r\n INNER JOIN Publishers p ON b.publisher_id = p.publisher_id";
                    SqlCommand command = new SqlCommand(selectSql, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var book = new Book
                            {
                                publisher_id = reader.GetInt32(0),
                                title = reader.GetString(1),
                                author_name = reader.GetString(2),
                                publisher_name = reader.GetString(3),
                                published_date = reader.GetString(4),
                                description = reader.GetString(5),

                            };

                            books.Add(book);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return books;
        }

        /// <summary>
        /// StoreBooksInDatabase() methods takes List<BookInfoModel> as parameter
        /// This method is used for storing the result of API or jsonPathFile to Database
        /// </summary>
        /// <param name="bookInfos"></param>
        /// <returns></returns>
        public async Task StoreBooksInDatabase(List<BookInfo> bookInfos)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (var bookInfo in bookInfos)
                {

                    int authorId = GetOrCreateAuthorId(connection, bookInfo.Author);


                    int publisherId = GetOrCreatePublisherId(connection, bookInfo.Publisher, bookInfo.PublishedDate);

                    int bookId = GetOrCreateBookId(connection);

                    string insertBookSql = "INSERT INTO Books (book_id, title, author_id, publisher_id, description) VALUES (@BookId, @Title, @AuthorId, @PublisherId, LEFT(@Description, 1000))";
                    SqlCommand insertBookCommand = new SqlCommand(insertBookSql, connection);
                    insertBookCommand.Parameters.AddWithValue("@BookId", bookId);
                    insertBookCommand.Parameters.AddWithValue("@Title", bookInfo.Title);
                    insertBookCommand.Parameters.AddWithValue("@AuthorId", authorId);
                    insertBookCommand.Parameters.AddWithValue("@PublisherId", publisherId);
                    insertBookCommand.Parameters.AddWithValue("@Description", bookInfo.Description);

                    insertBookCommand.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// GetOrCreateAuthorId() 
        /// This methods return the AuthorId if there exists some records else it returns the AuthorId i.e ,set as IDENTITY column in Table
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        public int GetOrCreateAuthorId(SqlConnection connection, string author)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            string selectAuthorSql = "SELECT author_id FROM Authors WHERE author_name = @AuthorName";
            using (SqlCommand authorCommand = new SqlCommand(selectAuthorSql, connection))
            {
                authorCommand.Parameters.AddWithValue("@AuthorName", author);

                object authorIdResult = authorCommand.ExecuteScalar();

                if (authorIdResult != null)
                {
                    return (int)authorIdResult;
                }
                else
                {

                    string insertAuthorSql = "INSERT INTO Authors (author_name) VALUES (@AuthorName); SELECT SCOPE_IDENTITY();";
                    SqlCommand insertAuthorCommand = new SqlCommand(insertAuthorSql, connection);
                    insertAuthorCommand.Parameters.AddWithValue("@AuthorName", author);

                    return Convert.ToInt32(insertAuthorCommand.ExecuteScalar());
                }
            }
        }


        /// <summary>
        /// GetOrCreatePublisherId()
        ///  This methods return the PublisherId if there exists some records else it returns the PublisherId i.e ,set as IDENTITY column in Table
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="publisherName"></param>
        /// <param name="publishedDate"></param>
        /// <returns></returns>
        public int GetOrCreatePublisherId(SqlConnection connection, string publisherName, string publishedDate)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            string selectPublisherSql = "SELECT publisher_id FROM Publishers WHERE publisher_name = @PublisherName AND published_date = @PublishedDate";
            using (SqlCommand publisherCommand = new SqlCommand(selectPublisherSql, connection))
            {
                publisherCommand.Parameters.AddWithValue("@PublisherName", publisherName);
                publisherCommand.Parameters.AddWithValue("@PublishedDate", publishedDate);

                object publisherIdResult = publisherCommand.ExecuteScalar();

                if (publisherIdResult != null)
                {
                    return (int)publisherIdResult;
                }
                else
                {

                    string insertPublisherSql = "INSERT INTO Publishers (publisher_name, published_date) VALUES (@PublisherName, @PublishedDate); SELECT SCOPE_IDENTITY();";
                    SqlCommand insertPublisherCommand = new SqlCommand(insertPublisherSql, connection);
                    insertPublisherCommand.Parameters.AddWithValue("@PublisherName", publisherName);
                    insertPublisherCommand.Parameters.AddWithValue("@PublishedDate", publishedDate);

                    return Convert.ToInt32(insertPublisherCommand.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// PostIntoBooks Method is used to add values into the database
        /// </summary>
        /// <param name="bookInfo"></param>
        /// <returns></returns>
        public BookInfoModel PostIntoBooks(BookInfoModel bookInfo)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();

                int bookId = GetOrCreateBookId(sqlConnection);
                int authId = GetOrCreateAuthorId(sqlConnection, bookInfo.author_name);
                int pubId = GetOrCreatePublisherId(sqlConnection, bookInfo.publisher_name, bookInfo.published_date);

                SqlCommand insertBookCommand = new SqlCommand("INSERT INTO Books(book_id, title, author_id, publisher_id, description)" +
                    "VALUES (@BookId, @Title, @Author_ID, @Publisher_ID, @Description)", sqlConnection);

                insertBookCommand.Parameters.AddWithValue("@BookId", bookId);
                insertBookCommand.Parameters.AddWithValue("@Title", bookInfo.title);
                insertBookCommand.Parameters.AddWithValue("@Author_ID", authId);
                insertBookCommand.Parameters.AddWithValue("@Publisher_ID", pubId);
                insertBookCommand.Parameters.AddWithValue("@Description", bookInfo.description);

                insertBookCommand.ExecuteNonQuery();

                sqlConnection.Close();

                var list = new BookInfoModel
                {
                    book_id = bookId,
                    title = bookInfo.title,
                    author_id = authId,
                    publisher_id = pubId,
                    description = bookInfo.description,
                    author_name = bookInfo.author_name,
                    publisher_name = bookInfo.publisher_name
                };

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while inserting the book: " + ex.Message);
            }
        }


        /// <summary>
        /// PutIntoBooks Method is used to update the values in the database
        /// </summary>
        /// <param name="bookInfo"></param>
        /// <returns></returns>
        public BookInfoModel PutIntoBooks(int bookId, BookInfoModel bookInfo)
        { 
            try
            {
                
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }
                List<BookInfoModel> list = new List<BookInfoModel>();
               
                int authId = GetOrCreateAuthorId(sqlConnection, bookInfo.author_name);
                int pubId = GetOrCreatePublisherId(sqlConnection, bookInfo.publisher_name, bookInfo.published_date);

                SqlCommand sqlCommand = new SqlCommand("UPDATE Books SET " +
                    "title = ISNULL(@Title, title) ," +
                   "author_id = ISNULL(@Author_ID, author_id), " +
                    "publisher_id = ISNULL(@Publisher_ID, publisher_id), " +
                    "description = ISNULL(@Description, description) " +
                    "WHERE book_id = @Id;", sqlConnection);

                sqlCommand.Parameters.AddWithValue("@Id", bookId);
                sqlCommand.Parameters.AddWithValue("@Title", bookInfo.title);
                sqlCommand.Parameters.AddWithValue("@Author_ID", authId);
                sqlCommand.Parameters.AddWithValue("@Publisher_ID", pubId);
                sqlCommand.Parameters.AddWithValue("@Description", bookInfo.description);

                //sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();

                var book = new BookInfoModel
                {
                    book_id = bookId,
                    title = bookInfo.title,
                    author_id = authId,
                    publisher_id = pubId,
                    description = bookInfo.description,
                    published_date = bookInfo.published_date,
                    author_name= bookInfo.author_name,
                    publisher_name= bookInfo.publisher_name
                };

                return book;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the book: " + ex.Message);
            }
           
        }


        /// <summary>
        /// GetOrCreateBookId used to return the maximum book_id 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public int GetOrCreateBookId(SqlConnection connection)
        {
            string selectMaxBookIdSql = "SELECT MAX(book_id) FROM Books";
            SqlCommand selectMaxBookIdCommand = new SqlCommand(selectMaxBookIdSql, connection);
            var maxId = selectMaxBookIdCommand.ExecuteScalar();
            if (maxId == DBNull.Value)
            {
                return 1;
            }
            else
            {
                return (int)maxId + 1;
            }
        }
    }
}


