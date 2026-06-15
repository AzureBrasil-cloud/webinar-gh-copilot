using BookStore.Web.Application.Services;
using BookStore.Web.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Controllers;

public class AuthorsController : Controller
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    public async Task<IActionResult> Index()
    {
        var authors = await _authorService.GetAllAsync();
        return View(authors);
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
        catch (InvalidOperationException ex)
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
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(author);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var author = await _authorService.GetByIdAsync(id);
        if (author is null) return NotFound();
        return View(author);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _authorService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            var author = await _authorService.GetByIdAsync(id);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(author);
        }
    }
}
