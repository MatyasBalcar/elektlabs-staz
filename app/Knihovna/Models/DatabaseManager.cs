using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System;
using Knihovna.Properties;

namespace Knihovna.Models
{
    public class DatabaseManager
    {
        private readonly DbContextOptions<AppDbContext>? _options;

        public static CultureInfo Culture { get; } = new("cs-CZ");

        public DatabaseManager(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public DatabaseManager() { }

        private AppDbContext CreateContext()
        {
            return _options != null ? new AppDbContext(_options) : new AppDbContext();
        }

        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using var context = new AppDbContext();
                if (!context.Database.CanConnect())
                {
                    errorMessage = Resources.DbFileNotFound;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = string.Format(Resources.DbConnectionError, ex.Message);
                return false;
            }
        }

        public List<Book> GetBooks(string? name = "", string? author = "", string? language = "", string? publisher = "")
        {
            using var context = CreateContext();
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

        public List<Author> GetAuthors(string? searchTerm = "", string? nationality = "")
        {
            using var context = CreateContext();
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

        public void SaveBook(Book book)
        {
            using var context = CreateContext();

            if (!string.IsNullOrWhiteSpace(book.ISBN))
            {
                bool isbnExists = context.Books.Any(b => b.ISBN == book.ISBN && b.BookId != book.BookId);

                if (isbnExists)
                {
                    throw new InvalidOperationException(string.Format(Resources.BookExistsISBN, book.ISBN));
                }
            }

            if (book.Language != null)
            {
                var existingLang = context.Languages
                    .FirstOrDefault(l => l.LanguageID == book.Language.LanguageID);

                if (existingLang != null)
                {
                    book.Language = existingLang;
                    book.LanguageId = existingLang.LanguageID;
                    context.Entry(existingLang).State = EntityState.Unchanged;
                }
                else if (book.Language.LanguageID == 0)
                {
                    context.Languages.Add(book.Language);
                }
            }
            else
            {
                book.LanguageId = null;
            }

            if (book.Publisher != null)
            {
                var existingPub = context.Publishers
                    .FirstOrDefault(p => p.PublisherID == book.Publisher.PublisherID);

                if (existingPub != null)
                {
                    book.Publisher = existingPub;
                    book.PublisherId = existingPub.PublisherID;
                    context.Entry(existingPub).State = EntityState.Unchanged;
                }
                else if (book.Publisher.PublisherID == 0)
                {
                    context.Publishers.Add(book.Publisher);
                }
            }
            else
            {
                book.PublisherId = null;
            }

            if (book.BookId == 0)
            {
                foreach (var author in book.Authors)
                {
                    if (author.AuthorId != 0)
                    {
                        context.Entry(author).State = EntityState.Unchanged;
                    }
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

        public void DeleteBook(int bookId)
        {
            using var context = CreateContext();
            var book = context.Books.Find(bookId);
            if (book == null) return;
            context.Books.Remove(book);
            context.SaveChanges();
        }

        public void DeleteAuthor(int authorId)
        {
            using var context = CreateContext();

            var booksToDelete = context.Books
                .Where(b => b.Authors.Any(a => a.AuthorId == authorId))
                .ToList();

            context.Books.RemoveRange(booksToDelete);

            var author = context.Authors.Find(authorId);
            if (author != null) context.Authors.Remove(author);

            context.SaveChanges();
        }

        public void SaveAuthor(Author author)
        {
            using var context = CreateContext();

            if (author.Nationality != null)
            {
                var existingNationality = context.Nationalities
                    .FirstOrDefault(n => n.NationalityID == author.Nationality.NationalityID);

                if (existingNationality != null)
                {
                    author.Nationality = existingNationality;
                    author.NationalityId = existingNationality.NationalityID;
                    context.Entry(existingNationality).State = EntityState.Unchanged;
                }
                else if (author.Nationality.NationalityID == 0)
                {
                    context.Nationalities.Add(author.Nationality);
                }
            }
            else
            {
                author.NationalityId = null;
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

        public List<Nationality> GetAllNationalities()
        {
            using var context = CreateContext();
            return context.Nationalities
                .ToList()
                .OrderBy(n => n.Name, StringComparer.Create(Culture, false))
                .ToList();
        }

        public List<Publisher> GetAllPublishers()
        {
            using var context = CreateContext();
            return context.Publishers
                .ToList()
                .OrderBy(p => p.Name, StringComparer.Create(Culture, false))
                .ToList();
        }

        public List<Language> GetAllLanguages()
        {
            using var context = CreateContext();
            return context.Languages
                .ToList()
                .OrderBy(l => l.Name, StringComparer.Create(Culture, false))
                .ToList();
        }
    }
}