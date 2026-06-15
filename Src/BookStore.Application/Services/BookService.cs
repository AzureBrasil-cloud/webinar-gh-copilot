using BookStore.Domain.Data;
using BookStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Services;

public class BookService : IBookService
{
    private readonly BookStoreContext _db;

    public BookService(BookStoreContext db)
    {
        _db = db;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _db.Books
            .Include(b => b.Author)
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _db.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book> CreateAsync(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (book.Price < 0)
        {
            throw new InvalidOperationException("Price cannot be negative.");
        }

        if (book.Stock < 0)
        {
            throw new InvalidOperationException("Stock cannot be negative.");
        }

        if (book.PublishedDate > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Published date cannot be in the future.");
        }

        var authorExists = await _db.Authors.AnyAsync(a => a.Id == book.AuthorId);
        if (!authorExists)
        {
            throw new InvalidOperationException("Author does not exist.");
        }

        book.Title = book.Title.Trim();
        book.Isbn = (book.Isbn ?? string.Empty).Trim();
        book.Genre = (book.Genre ?? string.Empty).Trim();
        book.Description = (book.Description ?? string.Empty).Trim();
        book.IsAvailable = book.Stock > 0;

        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateAsync(int id, Book book)
    {
        var existing = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (existing is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(book.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (book.Price < 0)
        {
            throw new InvalidOperationException("Price cannot be negative.");
        }

        if (book.Stock < 0)
        {
            throw new InvalidOperationException("Stock cannot be negative.");
        }

        if (book.PublishedDate > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Published date cannot be in the future.");
        }

        var authorExists = await _db.Authors.AnyAsync(a => a.Id == book.AuthorId);
        if (!authorExists)
        {
            throw new InvalidOperationException("Author does not exist.");
        }

        existing.Title = book.Title.Trim();
        existing.Isbn = (book.Isbn ?? string.Empty).Trim();
        existing.Genre = (book.Genre ?? string.Empty).Trim();
        existing.Description = (book.Description ?? string.Empty).Trim();
        existing.Price = book.Price;
        existing.Stock = book.Stock;
        existing.PublishedDate = book.PublishedDate;
        existing.AuthorId = book.AuthorId;
        existing.IsAvailable = book.Stock > 0;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (existing is null)
        {
            return false;
        }

        _db.Books.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
