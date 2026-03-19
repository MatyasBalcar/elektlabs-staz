using Microsoft.EntityFrameworkCore;
using System.IO;
using System;

namespace Knihovna.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options!)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string baseDir = AppContext.BaseDirectory;
                string dbPath = Path.Combine(baseDir, "db", "KNIHOVNADB.FDB");

                if (!File.Exists(dbPath))
                {
                    dbPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "db", "KNIHOVNADB.FDB"));
                }

                string newConnectionString = $"User=SYSDBA;Password=masterkey;Database=localhost:{dbPath};Charset=UTF8;";

                optionsBuilder.UseFirebird(newConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().ToTable("BOOKS");
            modelBuilder.Entity<Author>().ToTable("AUTHORS");
            modelBuilder.Entity<Language>().ToTable("LANGUAGES");
            modelBuilder.Entity<Publisher>().ToTable("PUBLISHERS");
            modelBuilder.Entity<Nationality>().ToTable("NATIONALITIES");

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToUpper());
                }
            }

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(a => a.Books)
                .UsingEntity(
                    "BOOKSAUTHORS",
                    l => l.HasOne(typeof(Author)).WithMany().HasForeignKey("AUTHORID"),
                    r => r.HasOne(typeof(Book)).WithMany().HasForeignKey("BOOKID")
                );
        }
    }
}