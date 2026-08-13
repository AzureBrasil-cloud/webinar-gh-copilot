using System.ComponentModel.DataAnnotations;

namespace BookStore.Web.Models.Api;

public class CreateBookRequest
{
    [Required]
    public string Title { get; init; } = string.Empty;
    public string Isbn { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public DateTime PublishedDate { get; init; }
    [Required]
    public int AuthorId { get; init; }
    public int NumberOfPages { get; init; }
}
