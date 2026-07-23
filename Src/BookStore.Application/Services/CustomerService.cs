using BookStore.Application.Common;
using BookStore.Domain.Data;
using BookStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly BookStoreContext _db;

    public CustomerService(BookStoreContext db)
    {
        _db = db;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _db.Customers
            .OrderBy(c => c.FullName)
            .ToListAsync();
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.Customers.OrderBy(c => c.FullName);
        var total = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<Customer> { Items = items, PageNumber = pageNumber, PageSize = pageSize, TotalItems = total };
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        var entity = Customer.Create(customer.FullName, customer.Email, customer.PhoneNumber);
        _db.Customers.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<Customer?> UpdateAsync(int id, Customer customer)
    {
        var existing = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null)
            return null;

        existing.Update(customer.FullName, customer.Email, customer.PhoneNumber);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);

        if (existing is null)
            return false;

        _db.Customers.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
