using BookStore.Domain.Data;
using BookStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Services;

public class AuthorService : IAuthorService
{
    private readonly BookStoreContext _db;

    public AuthorService(BookStoreContext db)
    {
        _db = db;
    }

    public async Task<List<Author>> GetAllAsync()
    {
        return await _db.Authors
            .Include(a => a.Books)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Author?> GetByIdAsync(int id)
    {
        return await _db.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Author> CreateAsync(Author author)
    {
        if (string.IsNullOrWhiteSpace(author.Name))
        {
            throw new InvalidOperationException("Author name is required.");
        }

        if (author.Name.Length > 150)
        {
            throw new InvalidOperationException("Author name must be at most 150 characters.");
        }

        if (author.BirthDate > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Birth date cannot be in the future.");
        }

        author.Name = author.Name.Trim();
        author.Nationality = (author.Nationality ?? string.Empty).Trim();
        author.Bio = (author.Bio ?? string.Empty).Trim();

        _db.Authors.Add(author);
        await _db.SaveChangesAsync();
        return author;
    }

    public async Task<Author?> UpdateAsync(int id, Author author)
    {
        var existing = await _db.Authors.FirstOrDefaultAsync(a => a.Id == id);
        if (existing is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(author.Name))
        {
            throw new InvalidOperationException("Author name is required.");
        }

        if (author.BirthDate > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Birth date cannot be in the future.");
        }

        existing.Name = author.Name.Trim();
        existing.Bio = (author.Bio ?? string.Empty).Trim();
        existing.Nationality = (author.Nationality ?? string.Empty).Trim();
        existing.BirthDate = author.BirthDate;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (existing is null)
        {
            return false;
        }

        if (existing.Books.Any())
        {
            throw new InvalidOperationException("Cannot delete an author who still has books.");
        }

        _db.Authors.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
