using Knihovna.Models;

namespace Knihovna.Services
{
    public static class Validator
    {
        public static string ValidateBook(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Name))
            {
                return "Název knihy je povinný.";
            }
            if (book.Name.Length > 255)
            {
                return "Název knihy je příliš dlouhý (maximum je 255 znaků).";
            }
            if (!string.IsNullOrWhiteSpace(book.ISBN) && book.ISBN.Length > 13)
            {
                return "ISBN nesmí být delší než 13 znaků. Zadávejte jej prosím bez pomlček a mezer.";
            }
            if (book.Authors == null || book.Authors.Count == 0)
            {
                return "Autor je povinný.";
            }
            if (book.Language == null)
            {
                return "Jazyk je povinný.";
            }
            if (book.Publisher == null)
            {
                return "Vydavatel je povinný.";
            }

            return string.Empty;
        }

        public static string ValidateAuthor(Author author)
        {
            if (string.IsNullOrWhiteSpace(author.FirstName))
            {
                return "Křestní jméno autora je povinné.";
            }
            if (author.LastName.Length > 100)
            {
                return "Křestní jméno autora ma maximalni delku 100 znaku.";
            }
            if (string.IsNullOrWhiteSpace(author.LastName))
            {
                return "Příjmení autora je povinné.";
            }
            if (author.LastName.Length > 100)
            {
                return "Příjmení autora ma maximalni delku 100 znaku.";
            }
            if (author.Nationality == null)
            {
                return "Narodnost je povinna";
            }

            return string.Empty;
        }
    }
}