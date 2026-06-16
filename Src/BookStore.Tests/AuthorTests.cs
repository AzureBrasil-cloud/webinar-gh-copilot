using BookStore.Domain.Entities;
using BookStore.Domain.Exceptions;
using FluentAssertions;

namespace BookStore.Tests;

public class AuthorTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Author ValidAuthor(DateTime? birthDate = null) =>
        Author.Create("George Orwell", "British novelist.", "British",
            birthDate ?? DateTime.UtcNow.AddYears(-50));

    // ── Author.Create – happy path ───────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsNonNullAuthor()
    {
        // Arrange & Act
        var author = Author.Create("Jane Austen", "English novelist.", "British",
            DateTime.UtcNow.AddYears(-40));

        // Assert
        author.Should().NotBeNull();
        author.Name.Should().Be("Jane Austen");
    }

    // ── Author.Create – validation ───────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_ThrowsDomainException(string name)
    {
        // Arrange & Act
        var act = () => Author.Create(name, "Bio", "British", DateTime.UtcNow.AddYears(-30));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Author name is required.");
    }

    [Fact]
    public void Create_WithFutureBirthDate_ThrowsDomainException()
    {
        // Arrange & Act
        var act = () => Author.Create("Valid Name", "Bio", "British",
            DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Birth date cannot be in the future.");
    }

    // ── Author.Age – computed property ───────────────────────────────────────

    [Fact]
    public void Age_WithBirthDateExactly30YearsAgo_Returns30()
    {
        // Arrange
        // AddYears(-30).AddDays(-1) ensures we are one day past the 30th birthday,
        // making the formula (TotalDays / 365.25) reliably floor to 30 regardless
        // of how many leap years fall in the range.
        var birthDate = DateTime.UtcNow.AddYears(-30).AddDays(-1);
        var author = Author.Create("Test Author", "Bio", "British", birthDate);

        // Act & Assert
        author.Age.Should().Be(30);
    }

    // ── Author.EnsureCanBeDeleted ─────────────────────────────────────────────

    [Fact]
    public void EnsureCanBeDeleted_WithBooks_ThrowsDomainException()
    {
        // Arrange
        var author = ValidAuthor();
        author.Books.Add(new Book()); // add a book directly to the collection

        // Act
        var act = () => author.EnsureCanBeDeleted();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot delete an author who still has books.");
    }
}
