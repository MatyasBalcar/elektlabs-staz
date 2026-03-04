using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Knihovna.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            /*
             * Connection string
             "User=SYSDBA;Password=masterkey;Database=localhost:C:\\Users\\balcarm\\Desktop\\staz\\app\\Knihovna\\db\\KNIHOVNADB.FDB;Charset=UTF8;";
             */
            DirectoryInfo pathtoDbFolder = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent;
            string newConnectionString = "User=SYSDBA;Password=masterkey;Database=localhost:"+ pathtoDbFolder + "\\db\\KNIHOVNADB.FDB;Charset=UTF8;";
            optionsBuilder.UseFirebird(newConnectionString);
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