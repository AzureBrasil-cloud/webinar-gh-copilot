using BookStore.Application.Services;
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
}
