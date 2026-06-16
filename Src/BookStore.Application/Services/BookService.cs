using BookStore.Application.Common;
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

    public async Task<PagedResult<Book>> GetPagedAsync(int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.Books.Include(b => b.Author).OrderBy(b => b.Title);
        var total = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<Book> { Items = items, PageNumber = pageNumber, PageSize = pageSize, TotalItems = total };
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _db.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book> CreateAsync(Book book, int numberOfPages)
    {
        var authorExists = await _db.Authors.AnyAsync(a => a.Id == book.AuthorId);
        if (!authorExists)
            throw new DomainException("Author does not exist.");

        var entity = Book.Create(book.Title, book.Isbn, book.Description, book.Genre, book.Price, book.Stock, book.PublishedDate, book.AuthorId, numberOfPages);
        _db.Books.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<Book?> UpdateAsync(int id, Book book, int numberOfPages)
    {
        var existing = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (existing is null)
            return null;

        var authorExists = await _db.Authors.AnyAsync(a => a.Id == book.AuthorId);
        if (!authorExists)
            throw new DomainException("Author does not exist.");

        existing.Update(book.Title, book.Isbn, book.Description, book.Genre, book.Price, book.Stock, book.PublishedDate, book.AuthorId, numberOfPages);
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
