namespace Knihovna.Models
{
    public class Book : ICloneable
    {
        public int BookID { get; set; }
        public string Name { get; set; }
        public DateTime? PublishDate { get; set; }
        public string ISBN { get; set; }
        public short Rating { get; set; }
        public bool HaveRead { get; set; }
        public string Description { get; set; }

        public int? LanguageID { get; set; }
        public virtual Language Language { get; set; }

        public int? PublisherID { get; set; }
        public virtual Publisher Publisher { get; set; }
        public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
        public object Clone()
        {
            return new Book
            {
                BookID = this.BookID,
                Name = this.Name,
                ISBN = this.ISBN,
                PublishDate = this.PublishDate,
                PublisherID = this.PublisherID,
                LanguageID = this.LanguageID,
                Language = this.Language,
                Publisher = this.Publisher,
                Description = this.Description,
                HaveRead = this.HaveRead,
                Rating = this.Rating,
                Authors = this.Authors != null ? new List<Author>(this.Authors) : new List<Author>()
            };
        }


    }
}