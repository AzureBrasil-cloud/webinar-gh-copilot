using System.ComponentModel.DataAnnotations;

namespace BookStore.Web.Models.Api;

public class CreateAuthorRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
}
