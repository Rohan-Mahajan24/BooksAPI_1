using books.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                //     RetrieveBooksFromDatabase(booksFromApi); // Add data from API to the database

                if (booksFromApi.Count > 0)
                {
                    // check
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
            var apiUrl = "https://www.googleapis.com/books/v1/volumes?q=kaplan%20test%20prep";

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
                                PublishedDate = item.volumeInfo.publishedDate
                                //description=

                            });
                        }

                        return bookInfos;
                    }
                }
                else
                {
                    Console.WriteLine($"API request failed with status code: {response.StatusCode}");
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
                    // Check if the author already exists in the Authors table
                    int authorId = GetOrCreateAuthorId(connection, bookInfo.Author);

                    // Check if the publisher already exists in the Publishers table
                    int publisherId = GetOrCreatePublisherId(connection, bookInfo.Publisher, bookInfo.PublishedDate);

                    // Insert into the Books table
                    string insertBookSql = "INSERT INTO Books (title, author_id, publisher_id, description) VALUES (@Title, @AuthorId, @PublisherId, @Description)";
                    SqlCommand insertBookCommand = new SqlCommand(insertBookSql, connection);
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
                // Author doesn't exist, insert into Authors table
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
                // Publisher doesn't exist, insert into Publishers table
                string insertPublisherSql = "INSERT INTO Publishers (publisher_name, published_date) VALUES (@PublisherName, @PublishedDate); SELECT SCOPE_IDENTITY();";
                SqlCommand insertPublisherCommand = new SqlCommand(insertPublisherSql, connection);
                insertPublisherCommand.Parameters.AddWithValue("@PublisherName", publisherName);
                insertPublisherCommand.Parameters.AddWithValue("@PublishedDate", publishedDate);

                return Convert.ToInt32(insertPublisherCommand.ExecuteScalar());
            }
        }


    }
}


