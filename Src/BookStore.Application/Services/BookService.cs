using BookStore.Domain.Data;
using BookStore.Domain.Entities;
using BookStore.Domain.Exceptions;
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
        var authorExists = await _db.Authors.AnyAsync(a => a.Id == book.AuthorId);
        if (!authorExists)
            throw new DomainException("Author does not exist.");

        var entity = Book.Create(book.Title, book.Isbn, book.Description, book.Genre, book.Price, book.Stock, book.PublishedDate, book.AuthorId);
        _db.Books.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<Book?> UpdateAsync(int id, Book book)
    {
        var existing = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (existing is null)
            return null;

        var authorExists = await _db.Authors.AnyAsync(a => a.Id == book.AuthorId);
        if (!authorExists)
            throw new DomainException("Author does not exist.");

        existing.Update(book.Title, book.Isbn, book.Description, book.Genre, book.Price, book.Stock, book.PublishedDate, book.AuthorId);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (existing is null)
            return false;

        _db.Books.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
