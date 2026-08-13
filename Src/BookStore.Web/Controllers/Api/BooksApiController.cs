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
            Items = result.Items.Select(ToDto).ToList(),
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
            return CreatedAtAction(nameof(Index), new { id = created.Id }, ToDto(withAuthor!));
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BookDto>> Update(int id, CreateBookRequest request)
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
            var updated = await _bookService.UpdateAsync(id, book, request.NumberOfPages);
            if (updated is null) return NotFound();

            var withAuthor = await _bookService.GetByIdAsync(id);
            return Ok(ToDto(withAuthor!));
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

    private static BookDto ToDto(Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Isbn = b.Isbn,
        Description = b.Description,
        Genre = b.Genre,
        Price = b.Price,
        Stock = b.Stock,
        NumberOfPages = b.NumberOfPages,
        IsAvailable = b.IsAvailable,
        PublishedDate = b.PublishedDate,
        AuthorId = b.AuthorId,
        AuthorName = b.Author?.Name ?? string.Empty
    };
}
