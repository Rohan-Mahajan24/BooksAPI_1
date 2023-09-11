using Newtonsoft.Json;

namespace books.Models
{
    public class BookInfo
    {
        public string Author { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string PublishedDate { get; set; }

        public string Description { get; set; }
    }

   
    public class GoogleBooksApiResponse
    {
        public string kind { get; set; }
        public int totalItems { get; set; }
        public List<GoogleBooksApiItem> items { get; set; }
    }

    public class GoogleBooksApiItem
    {
        public string kind { get; set; }
        public string id { get; set; }
        public string etag { get; set; }
        public string selfLink { get; set; }
        public GoogleBooksVolumeInfo volumeInfo { get; set; }
    }

    public class GoogleBooksVolumeInfo
    {
        public List<string> authors { get; set; }
        public string title { get; set; }
        public string publisher { get; set; }
        public string publishedDate { get; set; }

        public string description { get; set; }
    }
}


