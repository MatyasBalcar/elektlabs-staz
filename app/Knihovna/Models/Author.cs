using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Knihovna.Properties;

namespace Knihovna.Models
{
    public class Author : ObservableValidator, ICloneable
    {
        public int AuthorId { get; set; }

        private string? _firstName;

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.FirstNameRequired))]
        [MaxLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.FirstNameTooLong))]
        public string? FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value, true);
        }

        private string? _lastName;

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.LastNameRequired))]
        [MaxLength(100, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.LastNameTooLong))]
        public string? LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value, true);
        }

        public DateTime? DateOfBirth { get; set; }

        private Nationality? _nationality;

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.NationalityRequired))]
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
                if (firstError != null) return firstError.ErrorMessage ?? Resources.ValidationError;
            }

            return string.Empty;
        }
    }
}