using books.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Azure;

namespace books.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BooksController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBooks([FromQuery] bool seed = true)//seed=faalse
        {
            if (seed)
            {
                var booksFromDatabase = RetrieveBooksFromDatabase();

                if (booksFromDatabase.Count > 0)
                {
                    return Ok(booksFromDatabase);
                }
                else
                {
                    return NotFound("No books found in the database.");
                }
            }
            else
            {
                var booksFromApi = await FetchBooksFromApiAsync();
                //     RetrieveBooksFromDatabase(booksFromApi);

                if (booksFromApi.Count > 0)
                {
                    // await StoreBooksInDatabase(booksFromApi);
                    List<BookInfo> bookInfos = booksFromApi;
                    await StoreBooksInDatabase(bookInfos);
                    return Ok(booksFromApi);
                }
                else
                {
                    return NotFound("No books found from the API.");
                }
            }
        }
        [HttpGet("{bookId}")]
        public IActionResult GetBookById(int bookId)
        {
            var book = RetrieveBookByIdFromDatabase(bookId);

            if (book != null)
            {
                return Ok(book);
            }
            else
            {
                return NotFound($"Book with ID {bookId} not found.");
            }
        }

        private Book RetrieveBookByIdFromDatabase(int bookId)
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


        [HttpDelete("{bookId}")]
        public IActionResult DeleteBookById(int bookId)
        {
            if (DeleteBookFromDatabase(bookId))
            {
                return NoContent();
            }
            else
            {
                return NotFound($"Book with ID {bookId} not found.");
            }
        }

        private bool DeleteBookFromDatabase(int bookId)
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
        /// 
        /// </summary>
        /// <returns></returns>
        private List<Book> RetrieveBooksFromDatabase()
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
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<List<BookInfo>> FetchBooksFromApiAsync()
        {
            var httpClient = new HttpClient();
            var apiUrl = "https://www.bing.com/books/v1/volumes?q=kaplan%20test%20prep";


            try
            {
                var response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<GoogleBooksApiResponse>(content);

                    if (responseObject.items != null)
                    {
                        var bookInfos = new List<BookInfo>();

                        foreach (var item in responseObject.items)
                        {
                            bookInfos.Add(new BookInfo
                            {
                                //Id = item.id,
                                Author = item.volumeInfo.authors != null ? string.Join(", ", item.volumeInfo.authors) : "No author",
                                Title = item.volumeInfo.title,
                                Publisher = item.volumeInfo.publisher,
                                PublishedDate = item.volumeInfo.publishedDate,
                                Description = item.volumeInfo.description

                            });
                        }

                        return bookInfos;
                    }
                }
                else
                {
                    Console.WriteLine($"API request failed with status code: {response.StatusCode}");
                    var jsonFile = @"C:\Users\RMahajan\Desktop\BooksAPI_1\books\Controllers\books.json";
                    using (StreamReader reader = new StreamReader(jsonFile))
                    {
                        var jsonString = reader.ReadToEnd();
                        var bookInfos = JsonConvert.DeserializeObject<List<GoogleBooksApiResponse>>(jsonString);

                         var bookInfo = new List<BookInfo>();
                        foreach (var item in bookInfos[0].items)
                        {
                            bookInfo.Add(new BookInfo
                            {
                                Author = item.volumeInfo.authors != null ? string.Join(", ", item.volumeInfo.authors) : "No author",
                                Title = item.volumeInfo.title,
                                Publisher = item.volumeInfo.publisher,
                                PublishedDate = item.volumeInfo.publishedDate,
                                Description = item.volumeInfo.description
                            });
                        }

                        return bookInfo;
                        
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"API request failed with exception: {ex.Message}");
            }

            return new List<BookInfo>();    
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookInfos"></param>
        /// <returns></returns>

        private async Task StoreBooksInDatabase(List<BookInfo> bookInfos)
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
                    insertBookCommand.Parameters.AddWithValue("@Description", bookInfo.Description); // Use Description

                    insertBookCommand.ExecuteNonQuery();
                }
            }
        }

        private int GetOrCreateAuthorId(SqlConnection connection, string author)
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

        private int GetOrCreatePublisherId(SqlConnection connection, string publisherName, string publishedDate)
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
         int GetUniqueBookId(SqlConnection connection)
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


