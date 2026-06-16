using BookStore.Domain.Exceptions;

namespace BookStore.Domain.Entities;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Nationality { get; set; } = string.Empty;

    public ICollection<Book> Books { get; set; } = new List<Book>();

    public int Age => (int)Math.Floor((DateTime.UtcNow - BirthDate).TotalDays / 365.25);

    public static Author Create(string name, string bio, string nationality, DateTime birthDate)
    {
        Validate(name, birthDate);
        return new Author
        {
            Name = name.Trim(),
            Bio = (bio ?? string.Empty).Trim(),
            Nationality = (nationality ?? string.Empty).Trim(),
            BirthDate = birthDate
        };
    }

    public void Update(string name, string bio, string nationality, DateTime birthDate)
    {
        Validate(name, birthDate);
        Name = name.Trim();
        Bio = (bio ?? string.Empty).Trim();
        Nationality = (nationality ?? string.Empty).Trim();
        BirthDate = birthDate;
    }

    public void EnsureCanBeDeleted()
    {
        if (Books.Any())
            throw new DomainException("Cannot delete an author who still has books.");
    }

    private static void Validate(string name, DateTime birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Author name is required.");

        if (name.Length > 150)
            throw new DomainException("Author name must be at most 150 characters.");

        if (birthDate > DateTime.UtcNow)
            throw new DomainException("Birth date cannot be in the future.");
    }
}
