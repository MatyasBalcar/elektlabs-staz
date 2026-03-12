namespace Knihovna.Models
{
    public class Author : ICloneable
    {
        public int AuthorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public int? NationalityID { get; set; }
        public virtual Nationality Nationality { get; set; }

        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        public string FullName => $"{FirstName} {LastName}";
        public object Clone()
        {
            return new Author
            {
                AuthorId = this.AuthorId,
                FirstName = this.FirstName,
                LastName = this.LastName,
                DateOfBirth = this.DateOfBirth,
                NationalityID = this.NationalityID,
                Nationality = this.Nationality,
                Books = this.Books != null ? new List<Book>(this.Books) : new List<Book>()
            };
        }

        public string Validate()
        {
            if (string.IsNullOrWhiteSpace(FirstName)) return "Křestní jméno autora je povinné.";
            if (FirstName.Length > 100) return "Křestní jméno autora ma maximální delku 100 znaků.";

            if (string.IsNullOrWhiteSpace(LastName)) return "Příjmení autora je povinné.";
            if (LastName.Length > 100) return "Příjmení autora ma maximální delku 100 znaků.";

            if (Nationality == null && NationalityID == null) return "Národnost je povinná.";

            return string.Empty;
        }
    }
}