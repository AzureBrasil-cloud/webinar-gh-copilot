namespace BookStore.Web.Models.Api;

public class AuthorDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public int Age { get; init; }
    public int BooksCount { get; init; }
}
