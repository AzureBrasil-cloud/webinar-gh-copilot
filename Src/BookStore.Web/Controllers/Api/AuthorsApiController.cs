using BookStore.Application.Services;
using BookStore.Domain.Exceptions;
using BookStore.Web.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Controllers.Api;

[ApiController]
[Route("api/authors")]
public class AuthorsApiController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsApiController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AuthorDto>>> Index(int page = 1, int pageSize = 10)
    {
        var result = await _authorService.GetPagedAsync(page, pageSize);
        var dto = new PagedResultDto<AuthorDto>
        {
            Items = result.Items.Select(a => new AuthorDto
            {
                Id = a.Id,
                Name = a.Name,
                Nationality = a.Nationality,
                BirthDate = a.BirthDate,
                Age = a.Age,
                BooksCount = a.Books.Count
            }).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            HasPrevious = result.HasPrevious,
            HasNext = result.HasNext
        };

        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _authorService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
