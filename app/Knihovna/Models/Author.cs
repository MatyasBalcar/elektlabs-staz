namespace Knihovna.Models
{
    public class Author
    {
        public int AuthorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public int? NationalityID { get; set; }
        public virtual Nationality Nationality { get; set; }

        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        public string FullName => $"{FirstName} {LastName}";
        public Author Clone()
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
    }
}