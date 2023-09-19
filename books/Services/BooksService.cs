using books.Interfaces;
using books.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using books.Services;
using Microsoft.AspNetCore.Mvc;

namespace books.Services
{
    public class BookService : IBookService
    {
        private readonly IDatabaseService _databaseService;

        public BookService( IDatabaseService databaseService)
        {
            _databaseService = databaseService;
           
        }


        /// <summary>
        /// FetchBooksFromApiAsync() method reads the values from the api call and Stores it in the database
        /// </summary>
        /// <returns></returns>
        public async Task<List<BookInfo>> FetchBooksFromApiAsync()
        {
            var httpClient = new HttpClient();
             var apiUrl = "https://www.googleapis.com/books/v1/volumes?q=kaplan%20test%20prep";
             //var apiUrl = "https://www.googleapis.com/books/v1fgfg";
    
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

                                Author = item.volumeInfo.authors != null ? string.Join(", ", item.volumeInfo.authors) : "No author",
                                Title = item.volumeInfo.title,
                                Publisher = !string.IsNullOrEmpty(item.volumeInfo.publisher) ? item.volumeInfo.publisher : "NA",
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
                    await RetrieveBooksFromJson();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API request failed with exception: {ex.Message}");
            }

            return new List<BookInfo>();
        }

        /// <summary>
        /// RetrieveBooksFromJson() method reads the values from the JSON file and Stores it in the database
        /// </summary>
        /// <returns></returns>
        public async Task<List<BookInfo>> RetrieveBooksFromJson()
        {
            var jsonFilePath = @"C:\Users\RMahajan\Desktop\BooksAPI_1\books\Controllers\books.json";

            try
            {
                using (StreamReader reader = new StreamReader(jsonFilePath))
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RetrieveBooksFromJson: {ex.Message}");
                return new List<BookInfo>();
            }

        }

        /// <summary>
        /// SeedDatabaseAsync() method does the function calls for FetchBooksFromApiAsync(),RetrieveBooksFromJson(),RetrieveBooksFromDatabase() and this method is called in HTTP GET request
        /// </summary>
        /// <returns></returns>
        public async Task<List<BookInfo>> SeedDatabaseAsync()
        {
            List<Book> booksFromDatabase = _databaseService.RetrieveBooksFromDatabase();

            if (booksFromDatabase.Count == 0)
            {
                var booksFromApi = await FetchBooksFromApiAsync();

                if (booksFromApi != null && booksFromApi.Count > 0)
                {
                    await _databaseService.StoreBooksInDatabase(booksFromApi);

                    return booksFromApi.Select(book => new BookInfo
                    {
                        Title = book.Title,
                        Author = book.Author,
                        Publisher = book.Publisher,
                        PublishedDate = book.PublishedDate,
                        Description = book.Description
                    }).ToList();
                }
                else
                {   
                    var booksFromJson = await RetrieveBooksFromJson();

                    if (booksFromJson != null && booksFromJson.Count > 0)
                    {
                        await _databaseService.StoreBooksInDatabase(booksFromJson);
                        return booksFromJson.Select(book => new BookInfo
                        {
                            Title = book.Title,
                            Author = book.Author,
                            Publisher = book.Publisher,
                            PublishedDate = book.PublishedDate,
                            Description = book.Description
                        }).ToList();
                    }
                    else
                    {
                        return new List<BookInfo>(); 
                    }
                }
            }
            else
            {
                return booksFromDatabase.Select(book => new BookInfo
                {
                    Title = book.title,
                    Author = book.author_name,
                    Publisher = book.publisher_name,
                    PublishedDate = book.published_date,
                    Description = book.description
                }).ToList();
            }
        }


        /// <summary>
        /// GetBooksFromDatabase() method retrieves the value from database
        /// </summary>
        /// <returns></returns>
        public List<Book> GetBooksFromDatabase()
        {
            var booksFromDatabase = _databaseService.RetrieveBooksFromDatabase();

            if (booksFromDatabase.Count == 0)
            {
                return new List<Book>(); 
            }
            else
            {
                return booksFromDatabase; 
            }
        }


        /// <summary>
        ///  GetBookById method uses paramater bookId and returns the values of respective bookId
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public Book GetBookById(int bookId)
        {

            return _databaseService.RetrieveBookFromDatabase(bookId);
        }

        /// <summary>
        ///  DeleteBookById method uses paramater bookId and deletes the values of respective bookId
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public bool DeleteBookById(int bookId)
        {
            return _databaseService.DeleteBookByIdFromDatabase(bookId);
        }


        /// <summary>
        /// RetrieveBooksFromDatabase returns an method of IDatabaseService
        /// </summary>
        /// <returns></returns>
        List<Book> IBookService.RetrieveBooksFromDatabase()
        {
            return _databaseService.RetrieveBooksFromDatabase();
        }

        /// <summary>
        /// StoreBooksInDatabase returns an method of IDatabaseService
        /// </summary>
        /// <returns></returns>
        public Task StoreBooksInDatabase(List<BookInfo> bookInfos)
        {
            return _databaseService.StoreBooksInDatabase(bookInfos);
        }

        /// <summary>
        /// PostintoBooksTable returns an method of IDatabaseService
        /// </summary>
        /// <returns></returns>

        public BookInfoModel PostintoBooksTable(BookInfoModel bookInfo)
        {
            return _databaseService.PostIntoBooks(bookInfo);
        }

        /// <summary>
        /// PutintoBooksTable returns an method of IDatabaseService
        /// </summary>
        /// <returns></returns>
        public BookInfoModel PutintoBooksTable(int bookId, BookInfoModel bookInfo)
        {
            return _databaseService.PutIntoBooks(bookId, bookInfo);
        }
    }
}
