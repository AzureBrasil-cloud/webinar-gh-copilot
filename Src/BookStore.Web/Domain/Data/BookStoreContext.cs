using BookStore.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Web.Domain.Data;

public class BookStoreContext : DbContext
{
    public BookStoreContext(DbContextOptions<BookStoreContext> options) : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(150);
            entity.Property(a => a.Bio).HasMaxLength(2000);
            entity.Property(a => a.Nationality).HasMaxLength(100);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(250);
            entity.Property(b => b.Isbn).HasMaxLength(20);
            entity.Property(b => b.Description).HasMaxLength(2000);
            entity.Property(b => b.Genre).HasMaxLength(60);
            entity.Property(b => b.Price).HasPrecision(18, 2);

            entity.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
