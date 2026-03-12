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
                return "Název knihy je příliš dlouhý, maximální delka je 255 znaků.";
            }
            if (!string.IsNullOrWhiteSpace(book.ISBN))
            {
                string cleanIsbn = book.ISBN.Replace("-", "").Replace(" ", "").Trim();

                if (cleanIsbn.Length > 13 || !cleanIsbn.All(char.IsDigit))
                {
                    return "ISBN musí obsahovat přesně 13 číslic (pomlčky jsou povoleny, text ne).";
                }

                book.ISBN = cleanIsbn;
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
                return "Křestní jméno autora ma maximální delku 100 znaků.";
            }
            if (string.IsNullOrWhiteSpace(author.LastName))
            {
                return "Příjmení autora je povinné.";
            }
            if (author.LastName.Length > 100)
            {
                return "Příjmení autora ma maximální delku 100 znaků.";
            }
            if (author.Nationality == null)
            {
                return "Národnost je povinná";
            }

            return string.Empty;
        }
    }
}