using BookStore.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Controllers;

public class AuthorsController : Controller
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var result = await _authorService.GetPagedAsync(page, pageSize: 10);
        return View(result);
    }
}
