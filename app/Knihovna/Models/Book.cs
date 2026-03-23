using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Knihovna.Models
{
    public class Book : ObservableValidator, ICloneable
    {
        public int BookId { get; set; }

        private string? _name;

        [Required(ErrorMessage = "Název knihy je povinný.")]
        [MaxLength(255, ErrorMessage = "Název knihy je příliš dlouhý, maximální delka je 255 znaků.")]
        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value, true);
        }

        public DateTime? PublishDate { get; set; }

        private string? _isbn;

        public string? ISBN
        {
            get => _isbn;
            set => SetProperty(ref _isbn, value, true);
        }

        public short Rating { get; set; }
        public bool HaveRead { get; set; }
        public string? Description { get; set; }

        public int? LanguageId { get; set; }
        public virtual Language? Language { get; set; }

        public int? PublisherId { get; set; }
        public virtual Publisher? Publisher { get; set; }

        public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

        public object Clone()
        {
            return new Book
            {
                BookId = this.BookId,
                Name = this.Name,
                ISBN = this.ISBN,
                PublishDate = this.PublishDate,
                PublisherId = this.PublisherId,
                LanguageId = this.LanguageId,
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
            ValidateAllProperties();

            if (HasErrors)
            {
                var firstError = GetErrors().FirstOrDefault();
                if (firstError != null) return firstError.ErrorMessage ?? "Chyba validace.";
            }

            if (!string.IsNullOrWhiteSpace(ISBN))
            {
                string cleanIsbn = ISBN.Replace("-", "").Replace(" ", "").Trim();

                if (cleanIsbn.Length > 13 || cleanIsbn.Length < 10 || !cleanIsbn.All(char.IsDigit))
                    return "ISBN musí obsahovat od 10 do 13 číslic (pomlčky jsou povoleny, ale text ne).";

                _isbn = cleanIsbn;
            }

            if (Authors == null || Authors.Count == 0) return "Autor je povinný.";
            if (Language == null && LanguageId == null) return "Jazyk je povinný.";
            if (Publisher == null && PublisherId == null) return "Vydavatel je povinný.";

            return string.Empty;
        }
    }
}