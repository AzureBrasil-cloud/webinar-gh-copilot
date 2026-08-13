using BookStore.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Controllers;

public class BooksController : Controller
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var result = await _bookService.GetPagedAsync(page, pageSize: 10);
        return View(result);
    }
}
