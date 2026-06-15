using BookStore.Application.Common;
using BookStore.Application.Services;
using BookStore.Domain.Entities;
using BookStore.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Web.Controllers;

public class BooksController : Controller
{
    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;

    public BooksController(IBookService bookService, IAuthorService authorService)
    {
        _bookService = bookService;
        _authorService = authorService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var result = await _bookService.GetPagedAsync(page, pageSize: 10);
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book is null) return NotFound();
        return View(book);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateAuthorsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAuthorsAsync(book.AuthorId);
            return View(book);
        }

        try
        {
            await _bookService.CreateAsync(book);
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateAuthorsAsync(book.AuthorId);
            return View(book);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book is null) return NotFound();
        await PopulateAuthorsAsync(book.AuthorId);
        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateAuthorsAsync(book.AuthorId);
            return View(book);
        }

        try
        {
            var result = await _bookService.UpdateAsync(id, book);
            if (result is null) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateAuthorsAsync(book.AuthorId);
            return View(book);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book is null) return NotFound();
        return View(book);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _bookService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAuthorsAsync(int? selected = null)
    {
        var authors = await _authorService.GetAllAsync();
        ViewBag.Authors = new SelectList(authors, "Id", "Name", selected);
    }
}
