using books.Interfaces;
using books.Models;
using books.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace books.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;


        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// GetAllBooks implements HTTP GET method 
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetAllBooks([FromQuery] bool seed = true)
        {
            if (seed)
            {
                var result = await _bookService.SeedDatabaseAsync();
                if (result != null)
                {

                    return Ok(result);
                }
                else
                {
                    return NotFound("No books found from API or local JSON file.");
                }
            }
            else
            {
                var result = _bookService.GetBooksFromDatabase();
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound("No records found in the database.");
                }
            }
        }


        /// <summary>
        /// GetBookById implements HTTP GET{bookId} Request,where it fetches the value of book by using its book_id
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>

        [HttpGet("{bookId}")]
        public IActionResult GetBookById(int bookId)
        {
            var book = _bookService.GetBookById(bookId);

            if (book != null)
            {
                return Ok(book);
            }
            else
            {
                return NotFound($"Book with ID {bookId} not found.");
            }
        }

        /// <summary>
        /// PostBooks implements HTTP POST request,allows to insert new values to database
        /// </summary>
        /// <param name="bookInfo"></param>
        /// <returns></returns>

        [HttpPost]
        public IActionResult PostBooks(BookInfoModel bookInfo)
        {
            var postResult = _bookService.PostintoBooksTable(bookInfo);
            if (postResult != null)
            {
                    return Ok($"Insert is sucessfull for Book_ID {postResult.book_id} ");
            }
            else
            {
                return NoContent();
            }
        }

        /// <summary>
        /// PutintoBooks implements HTTP PUT{id} request where it allows to update the values in the database
        /// </summary>
        /// <param name="bookInfo"></param>
        /// <returns></returns>

        [HttpPut("{bookId}")]
        public IActionResult PutIntoBooks(int bookId, [FromBody] BookInfoModel bookInfo)

        {

            var putResult = _bookService.PutintoBooksTable(bookId, bookInfo);

            if (putResult != null)
            {
                return Ok(putResult);

            }
            else
            {
                return NoContent();
            }
        }

        /// <summary>
        /// DeleteBookById implements HTTP DELETE{bookId} request,where it allows to delete the particular book_id from the database
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>

        [HttpDelete("{bookId}")]
        public IActionResult DeleteBookById(int bookId)

        {
            bool deletionResult = _bookService.DeleteBookById(bookId);

            if (deletionResult)
            {
                return Ok($"Book with ID {bookId} has been deleted.");

            }
            else
            {
                return NotFound($"Book with ID {bookId} not found.");
            }
        }

    }
}