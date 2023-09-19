using books.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
namespace books.Interfaces
{

    /// <summary>
    /// IDatabaseService contains all the Interfaces that are implemented in DatabseService
    /// </summary>
    public interface IDatabaseService
    {
        Book RetrieveBookFromDatabase(int bookId);
        bool DeleteBookByIdFromDatabase(int bookId);
        List<Book> RetrieveBooksFromDatabase();
         Task StoreBooksInDatabase(List<BookInfo> bookInfos);
        int GetOrCreateAuthorId(SqlConnection connection, string author);
        int  GetOrCreatePublisherId(SqlConnection connection, string publisherName, string publishedDate);
        BookInfoModel PostIntoBooks(BookInfoModel bookInfo);
        BookInfoModel PutIntoBooks(int bookId, BookInfoModel bookInfo);
        int GetOrCreateBookId(SqlConnection connection);


    }
}
