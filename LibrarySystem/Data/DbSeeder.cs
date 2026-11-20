using LibrarySystem.Models;

namespace LibrarySystem.Data;

/// <summary>
/// Inserts a small set of sample rows so the demo has something to query.
/// It only runs when the database is empty, so running the program twice will not duplicate data.
/// </summary>
public static class DbSeeder
{
    public static void Seed(LibraryContext db)
    {
        if (db.Authors.Any())
        {
            return; // already seeded
        }

        // Dates are relative to "today" so the demo always has a realistic mix of
        // returned / currently-out / overdue loans, whenever you run it.
        var today = DateOnly.FromDateTime(DateTime.Today);

        // ---------- Authors, each with their books ----------
        // We add Books through the Author.Books collection. EF Core sees the relationship and sets
        // Book.AuthorId for us when saving: no need to assign foreign keys by hand.
        var orwell = new Author
        {
            Name = "George Orwell",
            BirthDate = new DateOnly(1903, 6, 25),
            Books =
            {
                new Book { Title = "1984",        ISBN = "978-0451524935" },
                new Book { Title = "Animal Farm", ISBN = "978-0451526342" }
            }
        };

        var austen = new Author
        {
            Name = "Jane Austen",
            BirthDate = new DateOnly(1775, 12, 16),
            Books =
            {
                new Book { Title = "Pride and Prejudice", ISBN = "978-0141439518" },
                new Book { Title = "Emma",                ISBN = "978-0141439587" }
            }
        };

        var mahfouz = new Author
        {
            Name = "Naguib Mahfouz",
            BirthDate = new DateOnly(1911, 12, 11),
            Books =
            {
                new Book { Title = "Palace Walk", ISBN = "978-0385264662" }
            }
        };

        db.Authors.AddRange(orwell, austen, mahfouz);

        // ---------- Borrowers ----------
        var sara  = new Borrower { Name = "Sara Hassan",  MembershipDate = today.AddYears(-3) };
        var omar  = new Borrower { Name = "Omar Khaled",  MembershipDate = today.AddYears(-1) };
        var laila = new Borrower { Name = "Laila Samir",  MembershipDate = today.AddMonths(-6) };

        db.Borrowers.AddRange(sara, omar, laila);

        // ---------- Loans (the many-to-many join rows) ----------
        // We set the navigation properties (Book / Borrower) rather than the numeric FKs, because
        // the Books and Borrowers above have no Ids yet. EF Core works the Ids out on SaveChanges.
        var nineteenEightyFour = orwell.Books.First(b => b.Title == "1984");
        var animalFarm         = orwell.Books.First(b => b.Title == "Animal Farm");
        var pride              = austen.Books.First(b => b.Title == "Pride and Prejudice");
        var palaceWalk         = mahfouz.Books.First();

        db.Loans.AddRange(
            // Returned loan.
            new Loan { Book = nineteenEightyFour, Borrower = sara, LoanDate = today.AddDays(-40), ReturnDate = today.AddDays(-28) },
            // Same book borrowed again by the same person later: allowed thanks to LoanDate in the key.
            new Loan { Book = nineteenEightyFour, Borrower = sara, LoanDate = today.AddDays(-5) },
            // Another borrower, same book: this is the "many borrowers per book" side.
            new Loan { Book = nineteenEightyFour, Borrower = omar, LoanDate = today.AddDays(-20), ReturnDate = today.AddDays(-10) },
            // Still out and overdue (more than 14 days).
            new Loan { Book = animalFarm, Borrower = omar, LoanDate = today.AddDays(-25) },
            // Still out, not overdue.
            new Loan { Book = pride, Borrower = laila, LoanDate = today.AddDays(-3) },
            // Returned.
            new Loan { Book = palaceWalk, Borrower = laila, LoanDate = today.AddDays(-30), ReturnDate = today.AddDays(-18) }
        );

        // One SaveChanges = one transaction: either everything is inserted or nothing is.
        db.SaveChanges();
    }
}
