using BookStore.Domain.Data;
using BookStore.Domain.Entities;

namespace BookStore.Domain.Seed;

public static class DataSeeder
{
    public static void Seed(BookStoreContext db)
    {
        SeedAuthors(db);
        SeedBooks(db);
        SeedCustomers(db);
        db.SaveChanges();
    }

    private static void SeedAuthors(BookStoreContext db)
    {
        if (db.Authors.Any())
            return;

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
    }

    private static void SeedBooks(BookStoreContext db)
    {
        if (db.Books.Any())
            return;

        static Book MakeBook(int id, int authorId, string title, string isbn, string genre, decimal price, int stock, DateTime publishedDate, string description, int numberOfPages)
        {
            var b = Book.Create(title, isbn, description, genre, price, stock, publishedDate, authorId, numberOfPages);
            b.Id = id;
            return b;
        }

        var books = new List<Book>
        {
            MakeBook(1,  1,  "1984",                          "9780451524935", "Dystopian",       14.99m, 25, new DateTime(1949, 6, 8),   "A dystopian social science fiction novel.",           328),
            MakeBook(2,  1,  "Animal Farm",                   "9780451526342", "Allegory",         9.99m, 40, new DateTime(1945, 8, 17),  "An allegorical novella.",                             112),
            MakeBook(3,  1,  "Homage to Catalonia",           "9780156421171", "Memoir",          12.50m,  8, new DateTime(1938, 4, 25),  "Personal account of the Spanish Civil War.",          232),
            MakeBook(4,  2,  "Pride and Prejudice",           "9780141439518", "Romance",         11.99m, 30, new DateTime(1813, 1, 28),  "A romantic novel of manners.",                        432),
            MakeBook(5,  2,  "Sense and Sensibility",         "9780141439662", "Romance",         10.99m, 18, new DateTime(1811, 10, 30), "Two sisters and their romantic experiences.",         374),
            MakeBook(6,  2,  "Emma",                          "9780141439587", "Romance",         10.99m,  0, new DateTime(1815, 12, 23), "A young woman with too much time to matchmake.",      474),
            MakeBook(7,  3,  "The Hobbit",                    "9780547928227", "Fantasy",         15.99m, 50, new DateTime(1937, 9, 21),  "Bilbo Baggins' adventure.",                           310),
            MakeBook(8,  3,  "The Fellowship of the Ring",    "9780547928210", "Fantasy",         18.99m, 22, new DateTime(1954, 7, 29),  "First volume of The Lord of the Rings.",              479),
            MakeBook(9,  3,  "The Two Towers",                "9780547928203", "Fantasy",         18.99m, 17, new DateTime(1954, 11, 11), "Second volume of The Lord of the Rings.",             415),
            MakeBook(10, 4,  "Murder on the Orient Express",  "9780062073495", "Mystery",         13.99m, 12, new DateTime(1934, 1, 1),   "A classic Hercule Poirot mystery.",                   256),
            MakeBook(11, 4,  "And Then There Were None",      "9780062073488", "Mystery",         13.99m, 14, new DateTime(1939, 11, 6),  "Ten strangers on an island.",                         264),
            MakeBook(12, 4,  "Death on the Nile",             "9780062073556", "Mystery",         12.99m,  9, new DateTime(1937, 11, 1),  "Poirot investigates a murder on a cruise.",           288),
            MakeBook(13, 5,  "The Old Man and the Sea",       "9780684801223", "Fiction",         11.50m, 20, new DateTime(1952, 9, 1),   "Story of an aging Cuban fisherman.",                  127),
            MakeBook(14, 5,  "A Farewell to Arms",            "9780684801469", "War",             13.50m, 11, new DateTime(1929, 9, 27),  "A love story set during World War I.",                332),
            MakeBook(15, 6,  "One Hundred Years of Solitude", "9780060883287", "Magical Realism", 16.99m, 16, new DateTime(1967, 5, 30),  "Multi-generational story of the Buendía family.",     417),
            MakeBook(16, 6,  "Love in the Time of Cholera",   "9780307389732", "Romance",         14.99m,  7, new DateTime(1985, 9, 5),   "An epic story of love and longing.",                  348),
            MakeBook(17, 7,  "Norwegian Wood",                "9780375704024", "Fiction",         14.50m, 13, new DateTime(1987, 9, 4),   "A nostalgic story of loss and sexuality.",            293),
            MakeBook(18, 7,  "Kafka on the Shore",            "9781400079278", "Magical Realism", 16.50m, 10, new DateTime(2002, 9, 12),  "Two intertwined narratives of self-discovery.",       505),
            MakeBook(19, 7,  "1Q84",                          "9780307476463", "Fiction",         19.99m,  5, new DateTime(2009, 5, 29),  "A complex parallel-world novel.",                     925),
            MakeBook(20, 8,  "The Shining",                   "9780307743657", "Horror",          14.99m, 24, new DateTime(1977, 1, 28),  "A family's winter at the haunted Overlook Hotel.",    447),
            MakeBook(21, 8,  "It",                            "9781501142970", "Horror",          17.99m, 19, new DateTime(1986, 9, 15),  "Children face a shape-shifting evil in Derry, Maine.", 960),
            MakeBook(22, 8,  "Misery",                        "9781501143106", "Thriller",        13.99m,  0, new DateTime(1987, 6, 8),   "A novelist held captive by his number one fan.",      338),
            MakeBook(23, 9,  "Foundation",                    "9780553293357", "Sci-Fi",          12.99m, 21, new DateTime(1951, 5, 1),   "The decline and fall of a galactic empire.",          255),
            MakeBook(24, 9,  "I, Robot",                      "9780553382563", "Sci-Fi",          11.99m, 26, new DateTime(1950, 12, 2),  "Nine stories about positronic robots.",               253),
            MakeBook(25, 10, "Mrs Dalloway",                  "9780156628709", "Modernist",       12.50m,  6, new DateTime(1925, 5, 14),  "A day in the life of Clarissa Dalloway.",             194),
            MakeBook(26, 10, "To the Lighthouse",             "9780156907392", "Modernist",       12.50m,  4, new DateTime(1927, 5, 5),   "The Ramsay family's visits to the Isle of Skye.",     209),
            MakeBook(27, 10, "Orlando",                       "9780156701600", "Modernist",       13.50m,  0, new DateTime(1928, 10, 11), "A poet who changes sex and lives for centuries.",     208),
        };

        db.Books.AddRange(books);
    }

    private static void SeedCustomers(BookStoreContext db)
    {
        if (db.Customers.Any())
            return;

        static Customer MakeCustomer(int id, string fullName, string email, string? phoneNumber, DateTime createdAt)
        {
            var c = Customer.Create(fullName, email, phoneNumber);
            c.Id = id;
            c.CreatedAt = createdAt;
            return c;
        }

        var customers = new List<Customer>
        {
            MakeCustomer(1, "Alice Johnson",  "alice.johnson@example.com",  "555-0101", new DateTime(2025, 1, 15, 9, 30, 0, DateTimeKind.Utc)),
            MakeCustomer(2, "Bob Smith",      "bob.smith@example.com",      "555-0102", new DateTime(2025, 2, 20, 14, 15, 0, DateTimeKind.Utc)),
            MakeCustomer(3, "Carol Williams", "carol.williams@example.com", null,       new DateTime(2025, 3, 10, 11, 0, 0, DateTimeKind.Utc)),
            MakeCustomer(4, "David Brown",    "david.brown@example.com",    "555-0104", new DateTime(2025, 4, 5, 16, 45, 0, DateTimeKind.Utc)),
            MakeCustomer(5, "Eva Garcia",     "eva.garcia@example.com",     "555-0105", new DateTime(2025, 5, 12, 8, 0, 0, DateTimeKind.Utc))
        };

        db.Customers.AddRange(customers);
    }
}
