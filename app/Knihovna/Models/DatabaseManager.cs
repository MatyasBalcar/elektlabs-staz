using Microsoft.EntityFrameworkCore;

namespace Knihovna.Models
{
    public class DatabaseManager
    {
        public List<Book> GetBooks(string name = "", string author = "", string language = "", string publisher = "")
        {
            using (var context = new AppDbContext())
            {
                var query = context.Books
                    .Include(b => b.Publisher)
                    .Include(b => b.Language)
                    .Include(b => b.Authors)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(b => b.Name.ToLower().Contains(name.ToLower()));
                }

                if (!string.IsNullOrWhiteSpace(author))
                {
                    query = query.Where(b => b.Authors.Any(a =>
                        (a.FirstName + " " + a.LastName).Contains(author)));
                }

                if (!string.IsNullOrWhiteSpace(language))
                {
                    query = query.Where(b => b.Language != null && b.Language.Name.Contains(language));
                }

                if (!string.IsNullOrWhiteSpace(publisher))
                {
                    query = query.Where(b => b.Publisher != null && b.Publisher.Name.Contains(publisher));
                }

                return query.OrderBy(b => b.Name).ToList();


            }
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
        public List<Author> GetAuthors(string searchTerm = "", string nationality = "")
        {
            using (var context = new AppDbContext())
            {
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

                return query.OrderBy(a => a.LastName).ToList();
            }
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

        public void SaveBook(Book book)
        {
            using (var context = new AppDbContext())
            {
                if (book.Language != null)
                {
                    var existingLang = context.Languages
                        .FirstOrDefault(l => l.Name.ToLower() == book.Language.Name.ToLower());

                    if (existingLang != null)
                    {
                        book.Language = existingLang;
                        book.LanguageID = existingLang.LanguageID;
                        context.Entry(existingLang).State = EntityState.Unchanged;
                    }
                    else
                    {
                        book.Language.LanguageID = 0; 
                    }
                }

                if (book.Publisher != null)
                {
                    var existingPub = context.Publishers
                        .FirstOrDefault(p => p.Name.ToLower() == book.Publisher.Name.ToLower());

                    if (existingPub != null)
                    {
                        book.Publisher = existingPub;
                        book.PublisherID = existingPub.PublisherID;
                        context.Entry(existingPub).State = EntityState.Unchanged;
                    }
                    else
                    {
                        book.Publisher.PublisherID = 0; 
                    }
                }

                if (book.BookID == 0)
                {
                    if (book.Authors != null)
                    {
                        foreach (var author in book.Authors)
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
                        .FirstOrDefault(b => b.BookID == book.BookID);

                    if (dbBook != null)
                    {
                        context.Entry(dbBook).CurrentValues.SetValues(book);

                        dbBook.Language = book.Language;
                        dbBook.LanguageID = book.LanguageID;
                        dbBook.Publisher = book.Publisher;
                        dbBook.PublisherID = book.PublisherID;

                        dbBook.Authors.Clear();
                        foreach (var author in book.Authors)
                        {
                            var dbAuthor = context.Authors.Find(author.AuthorID);
                            if (dbAuthor != null) dbBook.Authors.Add(dbAuthor);
                        }
                    }
                }

                context.SaveChanges();
            }
        }


        public void DeleteBook(int bookId)
        {
            using (var context = new AppDbContext())
            {
                var book = context.Books.Find(bookId);
                if (book != null)
                {
                    context.Books.Remove(book);
                    context.SaveChanges();
                }
            }
        }

        public bool CanDeleteAuthor(int authorId)
        {
            //obsolete
            using (var context = new AppDbContext())
            {
                //returns true if author doesnt have books, false otherwise
                return !context.Authors
                    .Any(a => a.AuthorID == authorId && a.Books.Any());
            }
        }

        public void DeleteAuthor(int authorId)
        {
            using (var context = new AppDbContext())
            {
                //also deletes authors books
                var booksToDelete = context.Books
                    .Where(b => b.Authors.Any(a => a.AuthorID == authorId))
                    .ToList();

                context.Books.RemoveRange(booksToDelete);

                var author = context.Authors.Find(authorId);
                if (author != null) context.Authors.Remove(author);

                context.SaveChanges();
            }
        }
        public void SaveAuthor(Author author)
        {
            using (var context = new AppDbContext())
            {
                if (author.Nationality != null)
                {

                    var existingNationality = context.Nationalities
                        .FirstOrDefault(n => n.Name.ToLower() == author.Nationality.Name.ToLower());

                    if (existingNationality != null)
                    {
                        author.Nationality = existingNationality;
                        author.NationalityID = existingNationality.NationalityID;

                        context.Entry(existingNationality).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                    }
                    else
                    {

                        author.Nationality.NationalityID = 0;
                    }
                }

                if (author.AuthorID == 0)
                {
                    context.Authors.Add(author);
                }
                else
                {
                    var dbAuthor = context.Authors.FirstOrDefault(a => a.AuthorID == author.AuthorID);
                    if (dbAuthor != null)
                    {

                        dbAuthor.NationalityID = author.NationalityID;
                        dbAuthor.Nationality = author.Nationality;
                        context.Entry(dbAuthor).CurrentValues.SetValues(author);
                    }
                }

                context.SaveChanges();
            }
        }

        public List<Nationality> GetAllNationalities()
        {
            using (var context = new AppDbContext())
            {
                return context.Nationalities.OrderBy(n => n.Name).ToList();
            }
        }

        public List<Publisher> GetAllPublishers()
        {
            using (var context = new AppDbContext())
            {
                return context.Publishers.OrderBy(p => p.Name).ToList();
            }
        }


        public List<Language> GetAllLanguages()
        {
            using (var context = new AppDbContext())
            {
                return context.Languages.OrderBy(l => l.Name).ToList();
            }
        }

    }
}