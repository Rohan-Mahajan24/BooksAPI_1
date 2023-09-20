using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using books.Controllers;
using books.Interfaces;
using books.Models;

namespace BooksTest.Controllers
{
    public class BooksControllerTests
    {
       
        private readonly BooksController _controller;
        private readonly Mock<IBookService> _bookServiceMock;
        private readonly Mock<IDatabaseService> _databaseServiceMock;

        public BooksControllerTests()
        {
            _bookServiceMock = new Mock<IBookService>();
            _databaseServiceMock = new Mock<IDatabaseService>();
            _controller = new BooksController(_bookServiceMock.Object);
        }

        [Fact]
        public async Task GetAllBooks_SeedTrue_ReturnsOk()
        {
            // Arrange
            _bookServiceMock.Setup(mock => mock.SeedDatabaseAsync())
                            .ReturnsAsync(new List<BookInfo> { new BookInfo() });

            // Act
            var result = await _controller.GetAllBooks(seed: true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var bookInfos = Assert.IsAssignableFrom<List<BookInfo>>(okResult.Value);
            Assert.Single(bookInfos); // Assuming one book was seeded
        }


        [Fact]
        public async Task GetAllBooks_SeedFalse_ReturnsOk()
        {
            // Arrange
            var booksFromDatabase = new List<Book>
            {
                    new Book
                    {
                        title = "Book Title",
                        author_name = "Author Name",
                        publisher_name = "Publisher Name",
                        published_date = "2023-09-19",
                        description = "Book Description"
                    }
            };
            _bookServiceMock.Setup(mock => mock.GetBooksFromDatabase())
                           .Returns(booksFromDatabase);

            // Act
            var result = await _controller.GetAllBooks(seed: false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var bookInfos = Assert.IsAssignableFrom<List<Book>>(okResult.Value);

           
            Assert.Single(bookInfos); 
            Assert.Equal("Book Title", bookInfos[0].title);
            Assert.Equal("Author Name", bookInfos[0].author_name);
            Assert.Equal("Publisher Name", bookInfos[0].publisher_name);
            Assert.Equal("2023-09-19", bookInfos[0].published_date);
            Assert.Equal("Book Description", bookInfos[0].description);
        }


        [Fact]
        public async Task GetAllBooks_SeedFalse_NoRecordsInDatabase_ReturnsNotFound()
        {
            // Arrange
            _bookServiceMock.Setup(mock => mock.SeedDatabaseAsync())
               .ReturnsAsync(new List<BookInfo> { new BookInfo() }); 

            // Act
            var result = await _controller.GetAllBooks(seed: false);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No records found in the database.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAllBooks_SeedTrue_JsonFileFound_ReturnsOk()
        {
            // Arrange
            _bookServiceMock.Setup<Task<List<BookInfo>>>(mock => mock.SeedDatabaseAsync())
                             .ReturnsAsync(new List<BookInfo> { new BookInfo() }); 

            // Act
            var result = await _controller.GetAllBooks(seed: true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var bookInfos = Assert.IsAssignableFrom<List<BookInfo>>(okResult.Value);

            // Assuming that one book was successfully seeded from the JSON file
            Assert.Single(bookInfos); 
        }


        [Fact]
        public async Task GetAllBooks_SeedTrue_ApiCallSucceeds_ReturnsOk()
        {
            // Arrange
            _bookServiceMock.Setup(mock => mock.SeedDatabaseAsync())
                            .ReturnsAsync(new List<BookInfo> { new BookInfo() });

            // Act
            var result = await _controller.GetAllBooks(seed: true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var bookInfos = Assert.IsAssignableFrom<List<BookInfo>>(okResult.Value);
            Assert.Single(bookInfos); 
        }


        [Fact]
        public async Task GetAllBooks_SeedTrue_WhenApiAndJsonFail_ReturnsNotFound()
        {
            // Arrange
            _bookServiceMock.Setup<Task<List<BookInfo>>>(mock => mock.SeedDatabaseAsync())
                             .ReturnsAsync((List<BookInfo>)null);
            // Act
            var result = await _controller.GetAllBooks(seed: true);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No books found from API or local JSON file.", notFoundResult.Value);
        }

    }
}
