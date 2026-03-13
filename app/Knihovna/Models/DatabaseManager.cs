using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Knihovna.Models
{
    public class DatabaseManager
    {
        public static CultureInfo Culture { get; } = new("cs-CZ");
        public static List<Book> GetBooks(string? name = "", string? author = "", string? language = "", string? publisher = "")
        {
            using var context = new AppDbContext();
            var query = context.Books
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.Authors)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(b => b.Name != null && b.Name.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                var searchAuthor = author.ToLower();
                query = query.Where(b => b.Authors.Any(a =>
                    (a.FirstName + " " + a.LastName).ToLower().Contains(searchAuthor)));
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(b => b.Language != null && b.Language.Name.Contains(language));
            }

            if (!string.IsNullOrWhiteSpace(publisher))
            {
                query = query.Where(b => b.Publisher != null && b.Publisher.Name.Contains(publisher));
            }

            return query.ToList()
                        .OrderBy(a => a.Name, StringComparer.Create(Culture, false))
                        .ToList();
        }

        //public List<Book> GetAllBooks()
        //{
        //    using (var context = new AppDbContext())
        //    {
        //        return context.Books
        //            .Include(b => b.Authors)
        //            .Include(b => b.Language)
        //            .Include(b => b.Publisher)
        //            .ToList();
        //    }
        //}

        public static List<Author> GetAuthors(string? searchTerm = "", string? nationality = "")
        {
            using var context = new AppDbContext();
            var query = context.Authors
                .Include(a => a.Books)     
                .Include(a => a.Nationality)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            { 
                query = query.Where(a => (a.FirstName + " " + a.LastName).ToLower().Contains(searchTerm.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(nationality))
            {
                query = query.Where(a => a.Nationality != null && a.Nationality.Name.Contains(nationality));

            }

            return query.ToList()
                        .OrderBy(a => a.FullName, StringComparer.Create(Culture, false))
                        .ToList();
        }

        //public List<Author> GetAllAuthors()
        //{
        //    using (var context = new AppDbContext())
        //    {
        //        return context.Authors
        //            .Include(a => a.Nationality)
        //            .Include(a => a.Books)
        //            .ToList();
        //    }
        //}

        public static void SaveBook(Book book)
        {
            using var context = new AppDbContext();
            var existingLang = context.Languages
                .FirstOrDefault(l => book.Language != null && l.LanguageID == book.Language.LanguageID);

            if (existingLang != null)
            {
                book.Language = existingLang;
                book.LanguageId = existingLang.LanguageID;
                context.Entry(existingLang).State = EntityState.Unchanged;
            }
            else
            {
                book.Language.LanguageID = 0; 
            }
                


            var existingPub = context.Publishers
                .FirstOrDefault(p => book.Publisher != null && p.PublisherID == book.Publisher.PublisherID);

            if (existingPub != null)
            {
                book.Publisher = existingPub;
                book.PublisherId = existingPub.PublisherID;
                context.Entry(existingPub).State = EntityState.Unchanged;
            }
            else
            {
                book.Publisher.PublisherID = 0; 
            }
                

            if (book.BookId == 0)
            {

                foreach (var author in book.Authors)
                {
                    context.Entry(author).State = EntityState.Unchanged;
                }

                context.Books.Add(book);
            }
            else
            {
                var dbBook = context.Books
                    .Include(b => b.Authors)
                    .FirstOrDefault(b => b.BookId == book.BookId);

                if (dbBook != null)
                {
                    context.Entry(dbBook).CurrentValues.SetValues(book);

                    dbBook.Language = book.Language;
                    dbBook.LanguageId = book.LanguageId;
                    dbBook.Publisher = book.Publisher;
                    dbBook.PublisherId = book.PublisherId;

                    dbBook.Authors.Clear();
                    foreach (var author in book.Authors)
                    {
                        var dbAuthor = context.Authors.Find(author.AuthorId);
                        if (dbAuthor != null) dbBook.Authors.Add(dbAuthor);
                    }
                }
            }

            context.SaveChanges();
        }


        public static void DeleteBook(int bookId)
        {
            using var context = new AppDbContext();
            var book = context.Books.Find(bookId);
            if (book == null) return;
            context.Books.Remove(book);
            context.SaveChanges();
        }

        //No longer used, but kept for future usage
        //public bool CanDeleteAuthor(int authorId)
        //{
        //    using var context = new AppDbContext();
        //    return !context.Authors
        //        .Any(a => a.AuthorID == authorId && a.Books.Any());
        //}

        public static void DeleteAuthor(int authorId)
        {
            using var context = new AppDbContext();
            //also deletes authors books
            var booksToDelete = context.Books
                .Where(b => b.Authors.Any(a => a.AuthorId == authorId))
                .ToList();

            context.Books.RemoveRange(booksToDelete);

            var author = context.Authors.Find(authorId);
            if (author != null) context.Authors.Remove(author);

            context.SaveChanges();
        }
        public static void SaveAuthor(Author author)
        {
            using var context = new AppDbContext();
            var existingNationality = context.Nationalities
                .FirstOrDefault(n => author.Nationality != null && n.NationalityID== author.Nationality.NationalityID);

            if (existingNationality != null)
            {
                author.Nationality = existingNationality;
                author.NationalityId = existingNationality.NationalityID;

                context.Entry(existingNationality).State = EntityState.Unchanged;
            }
            else
            {

                author.Nationality.NationalityID = 0;
            }
                

            if (author.AuthorId == 0)
            {
                context.Authors.Add(author);
            }
            else
            {
                var dbAuthor = context.Authors.FirstOrDefault(a => a.AuthorId == author.AuthorId);
                if (dbAuthor != null)
                {

                    dbAuthor.NationalityId = author.NationalityId;
                    dbAuthor.Nationality = author.Nationality;
                    context.Entry(dbAuthor).CurrentValues.SetValues(author);
                }
            }

            context.SaveChanges();
        }




        public static List<Nationality> GetAllNationalities()
        {
            using var context = new AppDbContext();
            return context.Nationalities
                .ToList()
                .OrderBy(n => n.Name, StringComparer.Create(Culture, false)) 
                .ToList();
        }

        public static List<Publisher> GetAllPublishers()
        {
            using var context = new AppDbContext();
            return context.Publishers
                .ToList()
                .OrderBy(p => p.Name, StringComparer.Create(Culture, false))
                .ToList();
        }

        public static List<Language> GetAllLanguages()
        {
            using var context = new AppDbContext();
            return context.Languages
                .ToList()
                .OrderBy(l => l.Name, StringComparer.Create(Culture, false))
                .ToList();
        }

}
}