# GitHub Copilot Webinar — .NET MVC

A hands-on webinar series that teaches **GitHub Copilot in VS Code** by progressively building and refactoring a small **ASP.NET Core 10 MVC** project (`BookStore.Web`).

Each module focuses on a different set of Copilot features (Ask / Edit / Agent modes, custom instructions, prompt files, agent skills, cloud agent, …) and is shipped as **its own git branch**, layered on top of the previous one.

## Repository layout

```
.
├── Src/
│   └── BookStore.Web/        ASP.NET Core 10 MVC project (Books + Authors CRUD)
│       ├── Domain/           Entities, EF Core In-Memory context, seed data
│       ├── Application/      Services
│       ├── Controllers/      MVC controllers
│       └── Views/            Razor views (Bootstrap 5)
├── Modules/
│   └── module1/
│       └── content.md        Webinar content for module 1
└── README.md
```

## Branching model

| Branch | Source | Content |
|---|---|---|
| `initial` | — | Base `BookStore.Web` project, intentionally containing two design issues to fix |
| `module/1` | `initial` | Result of applying Module 1 (Ask mode + custom instructions + Agent mode) |
| `module/2` | `module/1` | Result of applying Module 2 — TBD |
| … | … | … |

Each module's `content.md` is the script for the live session: what to type into Copilot, in which mode, and why.

## Run the app locally

```bash
cd Src/BookStore.Web
dotnet run
```

The app listens on the URL printed by `dotnet run` (defaults from `Properties/launchSettings.json`). Open it and navigate to **Books** or **Authors**. The in-memory database is seeded with **10 authors** and **27 books**.

## Modules

- [`Modules/module1/content.md`](Modules/module1/content.md) — GitHub Copilot fundamentals: Ask mode, custom instructions (general + file-scoped), Agent mode.
