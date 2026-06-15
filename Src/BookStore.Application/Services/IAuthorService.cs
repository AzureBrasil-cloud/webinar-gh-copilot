using BookStore.Domain.Entities;

namespace BookStore.Application.Services;

public interface IAuthorService
{
    Task<List<Author>> GetAllAsync();
    Task<Author?> GetByIdAsync(int id);
    Task<Author> CreateAsync(Author author);
    Task<Author?> UpdateAsync(int id, Author author);
    Task<bool> DeleteAsync(int id);
}
