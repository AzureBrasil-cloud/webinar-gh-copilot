using BookStore.Domain.Entities;
using BookStore.Domain.Exceptions;
using FluentAssertions;

namespace BookStore.Tests;

public class BookTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Book ValidBook(int numberOfPages = 200) =>
        Book.Create("Clean Code", "978-0132350884", "A handbook of agile software.", "Technology",
            29.99m, 10, DateTime.UtcNow.AddDays(-1), 1, numberOfPages);

    // ── Book.Create – NumberOfPages happy path ───────────────────────────────

    [Fact]
    public void Create_WithValidNumberOfPages_Succeeds()
    {
        // Arrange & Act
        var book = Book.Create("Clean Code", "978-0132350884", "A handbook of agile software.", "Technology",
            29.99m, 10, DateTime.UtcNow.AddDays(-1), 1, 300);

        // Assert
        book.Should().NotBeNull();
        book.NumberOfPages.Should().Be(300);
    }

    // ── Book.Create – NumberOfPages validation ───────────────────────────────

    [Fact]
    public void Create_WithNumberOfPagesZero_ThrowsDomainException()
    {
        // Arrange
        Action act = () => Book.Create("Clean Code", "978-0132350884", "A handbook of agile software.", "Technology",
            29.99m, 10, DateTime.UtcNow.AddDays(-1), 1, 0);

        // Act & Assert
        act.Should().Throw<DomainException>().WithMessage("NumberOfPages must be at least 1.");
    }

    [Fact]
    public void Create_WithNegativeNumberOfPages_ThrowsDomainException()
    {
        // Arrange
        Action act = () => Book.Create("Clean Code", "978-0132350884", "A handbook of agile software.", "Technology",
            29.99m, 10, DateTime.UtcNow.AddDays(-1), 1, -1);

        // Act & Assert
        act.Should().Throw<DomainException>().WithMessage("NumberOfPages must be at least 1.");
    }

    // ── Book.Update – NumberOfPages happy path ───────────────────────────────

    [Fact]
    public void Update_WithValidNumberOfPages_Succeeds()
    {
        // Arrange
        var book = ValidBook();

        // Act
        book.Update("Clean Code", "978-0132350884", "A handbook of agile software.", "Technology",
            29.99m, 10, DateTime.UtcNow.AddDays(-1), 1, 500);

        // Assert
        book.NumberOfPages.Should().Be(500);
    }
}
