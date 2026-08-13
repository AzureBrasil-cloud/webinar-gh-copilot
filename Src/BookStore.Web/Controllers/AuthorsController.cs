using BookStore.Application.Common;
using BookStore.Application.Services;
using BookStore.Domain.Entities;
using BookStore.Domain.Exceptions;
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

    public async Task<IActionResult> Details(int id)
    {
        var author = await _authorService.GetByIdAsync(id);
        if (author is null) return NotFound();
        return View(author);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Author author)
    {
        if (!ModelState.IsValid) return View(author);

        try
        {
            await _authorService.CreateAsync(author);
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(author);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var author = await _authorService.GetByIdAsync(id);
        if (author is null) return NotFound();
        return View(author);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Author author)
    {
        if (id != author.Id) return BadRequest();
        if (!ModelState.IsValid) return View(author);

        try
        {
            var result = await _authorService.UpdateAsync(id, author);
            if (result is null) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(author);
        }
    }
}
