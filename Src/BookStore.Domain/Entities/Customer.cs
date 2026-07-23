using System.Text.RegularExpressions;
using BookStore.Domain.Exceptions;

namespace BookStore.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public static Customer Create(string fullName, string email, string? phoneNumber)
    {
        Validate(fullName, email);
        return new Customer
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string fullName, string email, string? phoneNumber)
    {
        Validate(fullName, email);
        FullName = fullName.Trim();
        Email = email.Trim();
        PhoneNumber = phoneNumber?.Trim();
    }

    private static void Validate(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Customer full name is required.");

        if (fullName.Length > 150)
            throw new DomainException("Customer full name must be at most 150 characters.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Customer email is required.");

        if (!IsValidEmail(email.Trim()))
            throw new DomainException("Customer email is invalid.");
    }

    private static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
    }
}
