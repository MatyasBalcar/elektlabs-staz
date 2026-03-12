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
        public string Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) return "Název knihy je povinný.";
            if (Name.Length > 255) return "Název knihy je příliš dlouhý, maximální delka je 255 znaků.";

            if (!string.IsNullOrWhiteSpace(ISBN))
            {
                string cleanIsbn = ISBN.Replace("-", "").Replace(" ", "").Trim();

                if (cleanIsbn.Length != 13 || !cleanIsbn.All(char.IsDigit))
                    return "ISBN musí obsahovat přesně 13 číslic (pomlčky jsou povoleny, ale text ne).";

                ISBN = cleanIsbn;
            }

            if (Authors == null || Authors.Count == 0) return "Autor je povinný.";
            if (Language == null && LanguageID == null) return "Jazyk je povinný.";
            if (Publisher == null && PublisherID == null) return "Vydavatel je povinný.";

            return string.Empty;
        }

    }
}