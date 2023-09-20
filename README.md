# BooksAPI_1
BooksAPI is a versatile tool designed for interfacing with APIs and storing the obtained data in a SQL Server Database. It supports a range of HTTP methods, including GET, POST, PUT, and DELETE, and provides robust functionality for handling various scenarios.

Development Environment
1. Integrated Development Environment (IDE)
Visual Studio 2019 or later: This IDE is recommended for developing ASP.NET Core applications.
2. .NET Core SDK
.NET Core SDK 6 or later: Required for building and running .NET Core applications.

<h2>Database</h2>

1. Database Management System
Microsoft SQL Server: The project is configured to use Microsoft SQL Server as the relational database management system (RDBMS).
2. SQL Server Management Studio (Optional)
SQL Server Management Studio: This tool is optional but highly recommended for managing and interacting with the SQL Server database.


Web Development

1. ASP.NET Core
ASP.NET Core 3.1 or later: ASP.NET Core is the framework used for building the web application.
2. NuGet Packages
- Microsoft.Extensions.DependencyInjection: For dependency injection.
- Microsoft.AspNetCore.Mvc: For building web APIs.
- Microsoft.Extensions.Logging: For logging.
- System.data.sqlclient (4.8.5)


It performs Various the HTTP Request:

- GET ALL BOOKS : /api/books?seed=true
- GET BOOKS-BY-ID : /api/books/{bookId}
- POST : /api/books
- PUT BY BOOKSID : /api/books/{bookId}
- DELETE BOOKS-BY-ID : /api/books/{bookId}

<h2> UNIT TEST CASES</h2>

It performs all the unit test cases for GET ALL BOOKS api using XUNIT:

- GetAllBooks_SeedTrue_ReturnsOk
- GetAllBooks_SeedFalse_ReturnsOk
- GetAllBooks_SeedFalse_NoRecordsInDatabase_ReturnsNotFound
- GetAllBooks_SeedTrue_JsonFileFound_ReturnsOk
- GetAllBooks_SeedTrue_ApiCallSucceeds_ReturnsOk
- GetAllBooks_SeedTrue_WhenApiAndJsonFail_ReturnsNotFound

 
