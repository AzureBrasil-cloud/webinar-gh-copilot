using BookStore.Application.Services;
using BookStore.Domain.Entities;
using BookStore.Domain.Exceptions;
using BookStore.Web.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Controllers.Api;

[ApiController]
[Route("api/books")]
public class BooksApiController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksApiController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<BookDto>>> Index(int page = 1, int pageSize = 10)
    {
        var result = await _bookService.GetPagedAsync(page, pageSize);
        var dto = new PagedResultDto<BookDto>
        {
            Items = result.Items.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Isbn = b.Isbn,
                Genre = b.Genre,
                Price = b.Price,
                Stock = b.Stock,
                NumberOfPages = b.NumberOfPages,
                IsAvailable = b.IsAvailable,
                PublishedDate = b.PublishedDate,
                AuthorId = b.AuthorId,
                AuthorName = b.Author?.Name ?? string.Empty
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

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(CreateBookRequest request)
    {
        try
        {
            var book = new Book
            {
                Title = request.Title,
                Isbn = request.Isbn,
                Description = request.Description,
                Genre = request.Genre,
                Price = request.Price,
                Stock = request.Stock,
                PublishedDate = request.PublishedDate,
                AuthorId = request.AuthorId
            };
            var created = await _bookService.CreateAsync(book, request.NumberOfPages);
            var withAuthor = await _bookService.GetByIdAsync(created.Id);

            var dto = new BookDto
            {
                Id = withAuthor!.Id,
                Title = withAuthor.Title,
                Isbn = withAuthor.Isbn,
                Genre = withAuthor.Genre,
                Price = withAuthor.Price,
                Stock = withAuthor.Stock,
                NumberOfPages = withAuthor.NumberOfPages,
                IsAvailable = withAuthor.IsAvailable,
                PublishedDate = withAuthor.PublishedDate,
                AuthorId = withAuthor.AuthorId,
                AuthorName = withAuthor.Author?.Name ?? string.Empty
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
        var deleted = await _bookService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
