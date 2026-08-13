---
applyTo: "Src/BookStore.Web/**/*.cshtml,Src/BookStore.Web/Controllers/**,Src/BookStore.Web/Models/**,Src/BookStore.Web/Program.cs,Src/BookStore.Web/*.csproj"
---

# Web instructions (ASP.NET Core MVC hosting Vue)

How `BookStore.Web` serves a Vue 3 + PrimeVue front-end from classic MVC. Companion to
`frontend.instructions.md` (everything inside `ClientApp/`), `pagination.instructions.md` and
`vault.instructions.md`.

This is the **reference implementation** for migrating a classic MVC + Razor + Bootstrap app to
MVC + Vue.js. The last section lists what to reproduce in a new project.

## Mandatory vault workflow

1. Read `.github/instructions/vault.instructions.md`.
2. `/vault-search` before changing anything.
3. Do the work.
4. `/vault-write` afterwards (add new tags to `00_Index/Tags.md` first). Never hand-edit vault notes.

## The hosting model in one paragraph

MVC still owns routing, navigation and full page loads. Each Razor view renders an (almost) empty
`<div id="<feature>-app" data-*="...">`; `_Layout.cshtml` loads the single Vite bundle
(`~/dist/main.css`, `~/dist/main.js` as `type="module"`); `ClientApp/src/main.ts` finds whichever mount
points exist on the current page and mounts a Vue app into each. Data is **not** pushed through the Razor
model - the Vue components fetch it from JSON endpoints under `/api/*`. This is a multi-page app with Vue
islands, **not** an SPA: there is no client-side router and no shared client store.

```
Browser GET /Books
  └─ BooksController.Index          -> Views/Books/Index.cshtml  (mount div only)
       └─ _Layout.cshtml            -> ~/dist/main.css + ~/dist/main.js
            └─ main.ts mount()      -> BooksPage.vue mounted on #books-app
                 └─ fetch /api/books?page=1&pageSize=10  -> BooksApiController -> IBookService.GetPagedAsync
```

## Layout contract (`Views/Shared/_Layout.cshtml`)

- `<head>`: `~/dist/main.css`, then `~/css/site.css`, then the scoped-CSS bundle - all with `asp-append-version="true"` for cache busting.
- `<body class="h-screen overflow-hidden bg-surface-0">` - the shell layout itself uses Tailwind classes, which is why `ClientApp/src/style.css` declares `@source "../../Views";`.
- The global sidebar is a mount point in the layout, fed by the current route:
  ```cshtml
  var currentController = ViewContext.RouteData.Values["controller"]?.ToString() ?? "";
  <div id="sidebar-app"
       data-active="@currentController"
       data-home-url="@Url.Action("Index", "Home")"
       data-books-url="@Url.Action("Index", "Books")" ... ></div>
  ```
- Scripts, in order: `jquery.min.js` (only for unobtrusive validation on the remaining Razor forms), `~/dist/main.js` (`type="module"`), `~/js/site.js`, then `@await RenderSectionAsync("Scripts", required: false)`.
- The bundle is loaded **globally, once**, in the layout. Do **not** add per-view `<script src="~/dist/main.js">` inside a `@section Scripts` block - that would mount every app twice.

## Writing a Razor view that hosts a Vue component

```cshtml
@{
    ViewData["Title"] = "Books";
}

<div id="books-app"
     data-api-url="/api/books"
     data-authors-url="/api/authors"></div>
```

Rules:

- One `id="<feature>-app"` per feature; it must match the `mount(Component, "#<feature>-app")` call in `ClientApp/src/main.ts`.
- Pass every URL the component needs as a `data-*` attribute - use `@Url.Action(...)` for MVC routes so route changes never break the front-end. Only the JSON API paths are written literally (`/api/books`), matching the `[Route("api/...")]` attribute.
- `data-*` values are strings on the Vue side; never expect a typed value.
- Serialize domain objects into the view only when there is no alternative. The default is: markup in Razor, data over `/api`.
- Keep the view otherwise empty. Any conditional UI belongs in the Vue component.

### Current page inventory

| Route | View | Rendering |
|---|---|---|
| `/` | `Views/Home/Index.cshtml` | Vue (`Home.vue`) |
| `/Books` | `Views/Books/Index.cshtml` | Vue: list + Create/Edit/Details/Delete dialogs (full CRUD in Vue) |
| `/Authors` | `Views/Authors/Index.cshtml` | Vue: list + Create/Edit/Details/Delete dialogs (full CRUD in Vue) |
| `/Customers` | `Views/Customers/Index.cshtml` | Vue list; row actions **navigate to the Razor pages** below |
| `/Customers/Create|Edit|Details|Delete` | `Views/Customers/*.cshtml` | Classic Razor forms, Tailwind-styled via the `@layer components` classes in `ClientApp/src/style.css`, jQuery unobtrusive validation via `_ValidationScriptsPartial` |
| Any | `Views/Shared/_Layout.cshtml` | Vue sidebar island |

Customers is deliberately the "half-migrated" reference: it shows the intermediate state of a migration
(Vue list over Razor CRUD) next to the finished state (Books/Authors). When migrating a page fully, switch
`DataTableCommon` from `detailsUrl`/`editUrl`/`deleteUrl` (navigate) to `confirmDetails`/`confirmEdit`/`confirmDelete`
(emit + dialog), add the dialogs, and delete the Razor CRUD views.

`Views/Shared/_Pagination.cshtml` is a leftover from the Bootstrap era and is no longer referenced by any
view - all paging now happens in the PrimeVue paginator against the API.

## MVC controllers

Unchanged from classic MVC: thin, `Index(int page = 1)` calling `IXxxService.GetPagedAsync`, returning the
view. The Index views still declare `@model PagedResult<Entity>` even though the Vue table refetches over
`/api` - harmless, and it keeps server-side rendering available as a fallback. Do not add business logic
here (see `.github/copilot-instructions.md`).

## JSON API controllers (`Controllers/Api/`)

The bridge between MVC services and Vue components:

```csharp
[ApiController]
[Route("api/books")]
public class BooksApiController : ControllerBase
{
    [HttpGet]                        // GET /api/books?page=1&pageSize=10 -> PagedResultDto<BookDto>
    [HttpPost]                       // create      -> 201 + BookDto
    [HttpPut("{id}")]                // update      -> 200 + BookDto | 404
    [HttpDelete("{id}")]             // delete      -> 204 | 404
}
```

Conventions:

- **Reuse the existing application services** (`IBookService`, `IAuthorService`, `ICustomerService`). Never duplicate business rules, validation or paging logic in an API controller - see `pagination.instructions.md`.
- **Always map to a DTO** in `Models/Api/`. Entities have circular navigation properties (`Book.Author` / `Author.Books`) that break JSON serialization, and DTOs also flatten what the table needs (`AuthorName`, `BooksCount`, `Age`).
- Paged responses use `PagedResultDto<T>` (`Items`, `PageNumber`, `PageSize`, `TotalItems`, `TotalPages`, `HasPrevious`, `HasNext`) - it mirrors `PagedResult<T>` and the `PagedResult<T>` interface in `ClientApp/src/types.ts`.
- Catch `DomainException` and return `BadRequest(new { message = ex.Message })`. The front-end composables read `body.message` and show it in the dialog, so the domain message is the user-facing error.
- Return `NotFound()` for a missing id, `NoContent()` for a successful delete.
- Dropdown/lookup data gets its own endpoint (`GET /api/authors/options` -> `AuthorOptionDto[]`), never a full paged fetch.
- API controllers use `[ApiController]` + `ControllerBase` (no views) and are **not** anti-forgery protected, unlike the Razor POST actions which keep `[ValidateAntiForgeryToken]`. There is no authentication in this solution; add auth before exposing mutating endpoints publicly.
- Keep the JSON casing default (camelCase) - `types.ts` depends on it.

## Static assets

- `wwwroot/dist/` - Vite output (`main.js`, `main.css`, PrimeIcons fonts). **Generated, but committed**: `dotnet build`/`publish` never runs npm, so the deployed app would otherwise ship no front-end. Never hand-edit.
- `wwwroot/images/` - Figma-exported assets, referenced by absolute path from Vue (`/images/home/books.jpg`).
- `wwwroot/lib/` - jquery, jquery-validation, jquery-validation-unobtrusive only. **No bootstrap folder** - do not reintroduce one.
- `wwwroot/css/site.css` - base font sizing only; app styling belongs to Tailwind/PrimeVue.
- `Program.cs` uses `app.MapStaticAssets()` + `.WithStaticAssets()` (ASP.NET Core 10 static asset pipeline), so fingerprinting/compression is handled for files present at build time.

## Build, run, deploy

```bash
# front-end (after any ClientApp change)
cd Src/BookStore.Web/ClientApp && npm install && npm run build

# back-end
dotnet build Src/BookStore.slnx
dotnet run --project Src/BookStore.Web      # http://localhost:5045
dotnet test Src/BookStore.Tests/BookStore.Tests.csproj

# deploy (Azure App Service, zip deploy) - requires wwwroot/dist to be up to date and committed
./deploy.sh
```

VS Code tasks: `build` (default) and `watch` (`dotnet watch run`). `dotnet watch` reloads C#/Razor only -
front-end changes still need `npm run build`.

Persistence is EF Core **In-Memory** (`BookStoreDb`), re-seeded by `DataSeeder.Seed(db)` on every start:
data resets on restart, and every mutation done through the API disappears with the process.

## Checklist for a change touching this layer

- [ ] `/vault-search` before, `/vault-write` after.
- [ ] Controller stays thin; business rules stay in the domain.
- [ ] New/changed API shape mirrored in `ClientApp/src/types.ts`.
- [ ] Mount `id` matches a `mount(...)` call in `main.ts`; all URLs passed as `data-*`.
- [ ] `dotnet build Src/BookStore.slnx` clean; `npm run build` re-run if ClientApp changed.
- [ ] Page loaded in the browser and the network tab shows the expected `/api/*` call.

## Never

- Never load `~/dist/main.js` from a view - it is a layout-level global.
- Never add a second bundler entry point or a per-page bundle; `main.ts` is the single entry.
- Never reintroduce Bootstrap CSS/JS or `bi-*` icons (PrimeIcons `pi pi-*` only).
- Never serialize EF entities directly to JSON - always a DTO.
- Never move paging or validation logic into an API controller.
- Never add EF Core migrations or change the persistence provider.

## Porting to a new project

1. Keep MVC routing and controllers; add `Controllers/Api/` + `Models/Api/` DTOs over the existing services.
2. Add the `ClientApp/` Vite project (see `frontend.instructions.md` -> "Porting to a new project").
3. In `_Layout.cshtml`: drop the Bootstrap `<link>`/`<script>`, add `~/dist/main.css` and `~/dist/main.js` (`type="module"`, `asp-append-version`), keep jQuery only if Razor forms still validate with it, and add the sidebar/nav mount point.
4. Migrate page by page: Index view -> mount `<div>` with `data-*` URLs -> Vue list over the paged API -> then the CRUD dialogs -> then delete the obsolete Razor views and partials (including the old pagination partial).
5. Commit `wwwroot/dist` if the deployment pipeline does not run npm; otherwise add an npm build step to CI and gitignore it.
6. Record the resulting structure in that project's vault with `/vault-write`.
