using BookStore.Application.Common;
using BookStore.Domain.Entities;

namespace BookStore.Application.Services;

public interface IBookService
{
    Task<List<Book>> GetAllAsync();
    Task<PagedResult<Book>> GetPagedAsync(int pageNumber, int pageSize);
    Task<Book?> GetByIdAsync(int id);
    Task<Book> CreateAsync(Book book, int numberOfPages);
    Task<Book?> UpdateAsync(int id, Book book, int numberOfPages);
    Task<bool> DeleteAsync(int id);
}
