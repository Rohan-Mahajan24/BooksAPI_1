using books.Interfaces;
using books.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

        public bool AddPublisherToDatabase(Publisher publisher)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string insertPublisherSql = "INSERT INTO Publishers (publisher_name, published_date) VALUES (@PublisherName, @PublishedDate)";
                    SqlCommand insertPublisherCommand = new SqlCommand(insertPublisherSql, connection);
                    insertPublisherCommand.Parameters.AddWithValue("@PublisherName", publisher.publisher_name);
                    insertPublisherCommand.Parameters.AddWithValue("@PublishedDate", publisher.published_date);

                    int rowsAffected = insertPublisherCommand.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }


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


        public async Task StoreBooksInDatabase(List<BookInfo> bookInfos)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var bookInfo in bookInfos)
                {

                    int authorId = GetOrCreateAuthorId(connection, bookInfo.Author);


                    int publisherId = GetOrCreatePublisherId(connection, bookInfo.Publisher, bookInfo.PublishedDate);

                    int bookId = GetUniqueBookId(connection);

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


        public int GetOrCreateAuthorId(SqlConnection connection, string author)
        {
            string selectAuthorSql = "SELECT author_id FROM Authors WHERE author_name = @AuthorName";
            SqlCommand authorCommand = new SqlCommand(selectAuthorSql, connection);
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



        public int GetOrCreatePublisherId(SqlConnection connection, string publisherName, string publishedDate)
        {
            string selectPublisherSql = "SELECT publisher_id FROM Publishers WHERE publisher_name = @PublisherName AND published_date = @PublishedDate";
            SqlCommand publisherCommand = new SqlCommand(selectPublisherSql, connection);
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

        public int GetUniqueBookId(SqlConnection connection)
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


