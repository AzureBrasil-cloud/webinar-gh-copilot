using BookStore.Domain.Data;
using BookStore.Domain.Entities;

namespace BookStore.Domain.Seed;

public static class DataSeeder
{
    public static void Seed(BookStoreContext db)
    {
        if (db.Authors.Any() || db.Books.Any())
        {
            return;
        }

        var authors = new List<Author>
        {
            new() { Id = 1,  Name = "George Orwell",          Nationality = "British",   BirthDate = new DateTime(1903, 6, 25),  Bio = "English novelist and essayist, known for 1984 and Animal Farm." },
            new() { Id = 2,  Name = "Jane Austen",            Nationality = "British",   BirthDate = new DateTime(1775, 12, 16), Bio = "English novelist known for her social commentary." },
            new() { Id = 3,  Name = "J.R.R. Tolkien",         Nationality = "British",   BirthDate = new DateTime(1892, 1, 3),   Bio = "Author of The Lord of the Rings." },
            new() { Id = 4,  Name = "Agatha Christie",        Nationality = "British",   BirthDate = new DateTime(1890, 9, 15),  Bio = "Queen of crime fiction." },
            new() { Id = 5,  Name = "Ernest Hemingway",       Nationality = "American",  BirthDate = new DateTime(1899, 7, 21),  Bio = "American novelist, Nobel Prize laureate." },
            new() { Id = 6,  Name = "Gabriel García Márquez", Nationality = "Colombian", BirthDate = new DateTime(1927, 3, 6),   Bio = "Master of magical realism." },
            new() { Id = 7,  Name = "Haruki Murakami",        Nationality = "Japanese",  BirthDate = new DateTime(1949, 1, 12),  Bio = "Contemporary Japanese novelist." },
            new() { Id = 8,  Name = "Stephen King",           Nationality = "American",  BirthDate = new DateTime(1947, 9, 21),  Bio = "Prolific author of horror and suspense." },
            new() { Id = 9,  Name = "Isaac Asimov",           Nationality = "American",  BirthDate = new DateTime(1920, 1, 2),   Bio = "Science fiction author and biochemist." },
            new() { Id = 10, Name = "Virginia Woolf",         Nationality = "British",   BirthDate = new DateTime(1882, 1, 25),  Bio = "Modernist author and critic." }
        };

        db.Authors.AddRange(authors);

        var books = new List<Book>
        {
            new() { Id = 1,  AuthorId = 1, Title = "1984",                          Isbn = "9780451524935", Genre = "Dystopian",       Price = 14.99m, Stock = 25, PublishedDate = new DateTime(1949, 6, 8),   Description = "A dystopian social science fiction novel." },
            new() { Id = 2,  AuthorId = 1, Title = "Animal Farm",                   Isbn = "9780451526342", Genre = "Allegory",        Price = 9.99m,  Stock = 40, PublishedDate = new DateTime(1945, 8, 17),  Description = "An allegorical novella." },
            new() { Id = 3,  AuthorId = 1, Title = "Homage to Catalonia",           Isbn = "9780156421171", Genre = "Memoir",          Price = 12.50m, Stock = 8,  PublishedDate = new DateTime(1938, 4, 25),  Description = "Personal account of the Spanish Civil War." },
            new() { Id = 4,  AuthorId = 2, Title = "Pride and Prejudice",           Isbn = "9780141439518", Genre = "Romance",         Price = 11.99m, Stock = 30, PublishedDate = new DateTime(1813, 1, 28),  Description = "A romantic novel of manners." },
            new() { Id = 5,  AuthorId = 2, Title = "Sense and Sensibility",         Isbn = "9780141439662", Genre = "Romance",         Price = 10.99m, Stock = 18, PublishedDate = new DateTime(1811, 10, 30), Description = "Two sisters and their romantic experiences." },
            new() { Id = 6,  AuthorId = 2, Title = "Emma",                          Isbn = "9780141439587", Genre = "Romance",         Price = 10.99m, Stock = 0,  PublishedDate = new DateTime(1815, 12, 23), Description = "A young woman with too much time to matchmake." },
            new() { Id = 7,  AuthorId = 3, Title = "The Hobbit",                    Isbn = "9780547928227", Genre = "Fantasy",         Price = 15.99m, Stock = 50, PublishedDate = new DateTime(1937, 9, 21),  Description = "Bilbo Baggins' adventure." },
            new() { Id = 8,  AuthorId = 3, Title = "The Fellowship of the Ring",    Isbn = "9780547928210", Genre = "Fantasy",         Price = 18.99m, Stock = 22, PublishedDate = new DateTime(1954, 7, 29),  Description = "First volume of The Lord of the Rings." },
            new() { Id = 9,  AuthorId = 3, Title = "The Two Towers",                Isbn = "9780547928203", Genre = "Fantasy",         Price = 18.99m, Stock = 17, PublishedDate = new DateTime(1954, 11, 11), Description = "Second volume of The Lord of the Rings." },
            new() { Id = 10, AuthorId = 4, Title = "Murder on the Orient Express",  Isbn = "9780062073495", Genre = "Mystery",         Price = 13.99m, Stock = 12, PublishedDate = new DateTime(1934, 1, 1),   Description = "A classic Hercule Poirot mystery." },
            new() { Id = 11, AuthorId = 4, Title = "And Then There Were None",      Isbn = "9780062073488", Genre = "Mystery",         Price = 13.99m, Stock = 14, PublishedDate = new DateTime(1939, 11, 6),  Description = "Ten strangers on an island." },
            new() { Id = 12, AuthorId = 4, Title = "Death on the Nile",             Isbn = "9780062073556", Genre = "Mystery",         Price = 12.99m, Stock = 9,  PublishedDate = new DateTime(1937, 11, 1),  Description = "Poirot investigates a murder on a cruise." },
            new() { Id = 13, AuthorId = 5, Title = "The Old Man and the Sea",       Isbn = "9780684801223", Genre = "Fiction",         Price = 11.50m, Stock = 20, PublishedDate = new DateTime(1952, 9, 1),   Description = "Story of an aging Cuban fisherman." },
            new() { Id = 14, AuthorId = 5, Title = "A Farewell to Arms",            Isbn = "9780684801469", Genre = "War",             Price = 13.50m, Stock = 11, PublishedDate = new DateTime(1929, 9, 27),  Description = "A love story set during World War I." },
            new() { Id = 15, AuthorId = 6, Title = "One Hundred Years of Solitude", Isbn = "9780060883287", Genre = "Magical Realism", Price = 16.99m, Stock = 16, PublishedDate = new DateTime(1967, 5, 30),  Description = "Multi-generational story of the Buendía family." },
            new() { Id = 16, AuthorId = 6, Title = "Love in the Time of Cholera",   Isbn = "9780307389732", Genre = "Romance",         Price = 14.99m, Stock = 7,  PublishedDate = new DateTime(1985, 9, 5),   Description = "An epic story of love and longing." },
            new() { Id = 17, AuthorId = 7, Title = "Norwegian Wood",                Isbn = "9780375704024", Genre = "Fiction",         Price = 14.50m, Stock = 13, PublishedDate = new DateTime(1987, 9, 4),   Description = "A nostalgic story of loss and sexuality." },
            new() { Id = 18, AuthorId = 7, Title = "Kafka on the Shore",            Isbn = "9781400079278", Genre = "Magical Realism", Price = 16.50m, Stock = 10, PublishedDate = new DateTime(2002, 9, 12),  Description = "Two intertwined narratives of self-discovery." },
            new() { Id = 19, AuthorId = 7, Title = "1Q84",                          Isbn = "9780307476463", Genre = "Fiction",         Price = 19.99m, Stock = 5,  PublishedDate = new DateTime(2009, 5, 29),  Description = "A complex parallel-world novel." },
            new() { Id = 20, AuthorId = 8, Title = "The Shining",                   Isbn = "9780307743657", Genre = "Horror",          Price = 14.99m, Stock = 24, PublishedDate = new DateTime(1977, 1, 28),  Description = "A family's winter at the haunted Overlook Hotel." },
            new() { Id = 21, AuthorId = 8, Title = "It",                            Isbn = "9781501142970", Genre = "Horror",          Price = 17.99m, Stock = 19, PublishedDate = new DateTime(1986, 9, 15),  Description = "Children face a shape-shifting evil in Derry, Maine." },
            new() { Id = 22, AuthorId = 8, Title = "Misery",                        Isbn = "9781501143106", Genre = "Thriller",        Price = 13.99m, Stock = 0,  PublishedDate = new DateTime(1987, 6, 8),   Description = "A novelist held captive by his number one fan." },
            new() { Id = 23, AuthorId = 9, Title = "Foundation",                    Isbn = "9780553293357", Genre = "Sci-Fi",          Price = 12.99m, Stock = 21, PublishedDate = new DateTime(1951, 5, 1),   Description = "The decline and fall of a galactic empire." },
            new() { Id = 24, AuthorId = 9, Title = "I, Robot",                      Isbn = "9780553382563", Genre = "Sci-Fi",          Price = 11.99m, Stock = 26, PublishedDate = new DateTime(1950, 12, 2),  Description = "Nine stories about positronic robots." },
            new() { Id = 25, AuthorId = 10, Title = "Mrs Dalloway",                 Isbn = "9780156628709", Genre = "Modernist",       Price = 12.50m, Stock = 6,  PublishedDate = new DateTime(1925, 5, 14),  Description = "A day in the life of Clarissa Dalloway." },
            new() { Id = 26, AuthorId = 10, Title = "To the Lighthouse",            Isbn = "9780156907392", Genre = "Modernist",       Price = 12.50m, Stock = 4,  PublishedDate = new DateTime(1927, 5, 5),   Description = "The Ramsay family's visits to the Isle of Skye." },
            new() { Id = 27, AuthorId = 10, Title = "Orlando",                      Isbn = "9780156701600", Genre = "Modernist",       Price = 13.50m, Stock = 0,  PublishedDate = new DateTime(1928, 10, 11), Description = "A poet who changes sex and lives for centuries." }
        };

        db.Books.AddRange(books);
        db.SaveChanges();
    }
}
