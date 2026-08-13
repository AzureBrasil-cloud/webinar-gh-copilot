using BookStore.Application.Services;
using BookStore.Domain.Entities;
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

    [HttpGet("options")]
    public async Task<ActionResult<List<AuthorOptionDto>>> Options()
    {
        var authors = await _authorService.GetAllAsync();
        var dto = authors.Select(a => new AuthorOptionDto { Id = a.Id, Name = a.Name }).ToList();
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create(CreateAuthorRequest request)
    {
        try
        {
            var author = new Author
            {
                Name = request.Name,
                Bio = request.Bio,
                Nationality = request.Nationality,
                BirthDate = request.BirthDate
            };
            var created = await _authorService.CreateAsync(author);

            var dto = new AuthorDto
            {
                Id = created.Id,
                Name = created.Name,
                Nationality = created.Nationality,
                BirthDate = created.BirthDate,
                Age = created.Age,
                BooksCount = 0
            };
            return CreatedAtAction(nameof(Index), new { id = dto.Id }, dto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
