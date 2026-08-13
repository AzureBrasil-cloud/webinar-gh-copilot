namespace BookStore.Web.Models.Api;

public class BookDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Isbn { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public int NumberOfPages { get; init; }
    public bool IsAvailable { get; init; }
    public DateTime PublishedDate { get; init; }
    public int AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
}
