namespace books.Models
{
    public class BookInfoModel
    {

        public int book_id { get; set; }

        public string title { get; set; }

        public int author_id { get; set; }

        public string author_name { get; set; }

        public int publisher_id { get; set; }

        public string publisher_name { get; set; }
        public string description { get; set; }
      
        public string published_date { get; set; }
    }
}
