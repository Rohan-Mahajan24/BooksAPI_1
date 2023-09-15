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



        // getting values from book_id

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


        // Deleting Value from BOOK_ID
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