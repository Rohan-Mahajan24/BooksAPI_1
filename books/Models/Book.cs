namespace books.Models
{
    public class Book
    {
        public string title { get; set; }
       
        public string publisher_name { get; set; }
        public string published_date { get; set; }

        public string description { get; set; }
        public int  publisher_id { get; set; }

        public string author_name { get; set; }
    }

}
