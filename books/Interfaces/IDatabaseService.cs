using books.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
namespace books.Interfaces
{
    public interface IDatabaseService
    {
        Book RetrieveBookFromDatabase(int bookId);
        bool DeleteBookByIdFromDatabase(int bookId);
        List<Book> RetrieveBooksFromDatabase();
        bool AddPublisherToDatabase(Publisher publisher);


         Task StoreBooksInDatabase(List<BookInfo> bookInfos);
        int GetOrCreateAuthorId(SqlConnection connection, string author);
        int  GetOrCreatePublisherId(SqlConnection connection, string publisherName, string publishedDate);
        int GetUniqueBookId(SqlConnection connection);


    }
}
