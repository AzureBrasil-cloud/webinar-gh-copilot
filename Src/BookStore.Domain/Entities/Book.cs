using BookStore.Domain.Exceptions;

namespace BookStore.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime PublishedDate { get; set; }
    public bool IsAvailable => Stock > 0;

    public int AuthorId { get; set; }
    public Author? Author { get; set; }

    public static Book Create(string title, string isbn, string description, string genre, decimal price, int stock, DateTime publishedDate, int authorId)
    {
        Validate(title, price, stock, publishedDate);
        return new Book
        {
            Title = title.Trim(),
            Isbn = (isbn ?? string.Empty).Trim(),
            Description = (description ?? string.Empty).Trim(),
            Genre = (genre ?? string.Empty).Trim(),
            Price = price,
            Stock = stock,
            PublishedDate = publishedDate,
            AuthorId = authorId
        };
    }

    public void Update(string title, string isbn, string description, string genre, decimal price, int stock, DateTime publishedDate, int authorId)
    {
        Validate(title, price, stock, publishedDate);
        Title = title.Trim();
        Isbn = (isbn ?? string.Empty).Trim();
        Description = (description ?? string.Empty).Trim();
        Genre = (genre ?? string.Empty).Trim();
        Price = price;
        Stock = stock;
        PublishedDate = publishedDate;
        AuthorId = authorId;
    }

    private static void Validate(string title, decimal price, int stock, DateTime publishedDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        if (price < 0)
            throw new DomainException("Price cannot be negative.");

        if (stock < 0)
            throw new DomainException("Stock cannot be negative.");

        if (publishedDate > DateTime.UtcNow)
            throw new DomainException("Published date cannot be in the future.");
    }
}
