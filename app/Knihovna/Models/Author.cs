using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Knihovna.Models
{
    public class Author : ObservableValidator, ICloneable
    {
        public int AuthorId { get; set; }

        private string? _firstName;

        [Required(ErrorMessage = "Křestní jméno autora je povinné.")]
        [MaxLength(100, ErrorMessage = "Křestní jméno autora má maximální délku 100 znaků.")]
        public string? FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value, true);
        }

        private string? _lastName;

        [Required(ErrorMessage = "Příjmení autora je povinné.")]
        [MaxLength(100, ErrorMessage = "Příjmení autora má maximální délku 100 znaků.")]
        public string? LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value, true);
        }

        public DateTime? DateOfBirth { get; set; }

        private Nationality? _nationality;

        [Required(ErrorMessage = "Národnost je povinná.")]
        public virtual Nationality? Nationality
        {
            get => _nationality;
            set => SetProperty(ref _nationality, value, true);
        }

        public int? NationalityId { get; set; }

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
                NationalityId = this.NationalityId,
                Nationality = this.Nationality,
                Books = this.Books != null ? new List<Book>(this.Books) : new List<Book>()
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

            return string.Empty;
        }
    }
}