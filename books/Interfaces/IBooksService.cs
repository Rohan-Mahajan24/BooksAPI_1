using books.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;

namespace books.Interfaces
{

    /// <summary>
    /// IBookService contains all the Interfaces that are implemented in BooksService
    /// </summary>
    public interface IBookService
    {
        Task<List<BookInfo>> FetchBooksFromApiAsync();
       
        Book GetBookById(int bookId);
         bool DeleteBookById(int bookId);
        Task<List<BookInfo>> RetrieveBooksFromJson();
        List<Book> RetrieveBooksFromDatabase();

        Task StoreBooksInDatabase(List<BookInfo> bookInfos);

        Task<List<BookInfo>> SeedDatabaseAsync();
        List<Book> GetBooksFromDatabase();

        BookInfoModel PostintoBooksTable(BookInfoModel bookInfo);

        BookInfoModel PutintoBooksTable(int bookId, BookInfoModel bookInfo);



    }
}
