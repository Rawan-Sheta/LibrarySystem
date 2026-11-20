using LibrarySystem.Data;
using Microsoft.EntityFrameworkCore;

// =============================================================================
// Library System demo
// Creates the database, seeds sample data, then shows how navigation properties
// let us query the relationships without writing manual JOINs.
// =============================================================================

// -----------------------------------------------------------------------------
// CONNECTION STRING
// Default: SQL Server LocalDB, which ships with Visual Studio on Windows (no setup).
// Using full SQL Server / Docker / Linux / macOS? Replace it, for example:
//   "Server=localhost,1433;Database=LibrarySystemDb;User Id=sa;Password=<your password>;TrustServerCertificate=True"
// -----------------------------------------------------------------------------
const string connectionString =
    "Server=(localdb)\\MSSQLLocalDB;Database=LibrarySystemDb;Trusted_Connection=True;TrustServerCertificate=True";

var options = new DbContextOptionsBuilder<LibraryContext>()
    .UseSqlServer(connectionString)
    .Options;

// 'using' disposes the context (and closes the connection) when Main ends.
using var db = new LibraryContext(options);

// EnsureCreated builds the schema from our model if the database does not exist yet.
// It is perfect for demos. For real projects use migrations instead (see README.md).
db.Database.EnsureCreated();

DbSeeder.Seed(db);

var today = DateOnly.FromDateTime(DateTime.Today);

// -----------------------------------------------------------------------------
// 1) MANY-TO-ONE: from each Book up to its Author.
//    Include() tells EF Core to JOIN the Authors table and fill book.Author.
//    AsNoTracking() = read-only query, faster because EF Core skips change tracking.
// -----------------------------------------------------------------------------
Console.WriteLine("=== 1. Books and their authors (Many-to-One) ===");
var books = db.Books
    .AsNoTracking()
    .Include(b => b.Author)
    .OrderBy(b => b.Title)
    .ToList();

foreach (var book in books)
{
    Console.WriteLine($"  {book.Title,-22} by {book.Author.Name}  (ISBN {book.ISBN})");
}

// -----------------------------------------------------------------------------
// 2) ONE-TO-MANY: from each Author down to their Books.
//    Select() projects into a small anonymous type, so EF Core only fetches the columns we use.
// -----------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== 2. Authors and their books (One-to-Many) ===");
var authors = db.Authors
    .AsNoTracking()
    .OrderBy(a => a.Name)
    .Select(a => new
    {
        a.Name,
        a.BirthDate,
        Titles = a.Books.Select(b => b.Title).ToList()
    })
    .ToList();

foreach (var author in authors)
{
    Console.WriteLine($"  {author.Name} (born {author.BirthDate:yyyy-MM-dd}): {string.Join(", ", author.Titles)}");
}

// -----------------------------------------------------------------------------
// 3) MANY-TO-MANY with the skip navigation: Book -> Borrowers.
//    We never mention the Loan table, yet EF Core joins through it for us.
// -----------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== 3. Who has ever borrowed each book (Many-to-Many, skip navigation) ===");
var borrowersPerBook = db.Books
    .AsNoTracking()
    .OrderBy(b => b.Title)
    .Select(b => new
    {
        b.Title,
        Borrowers = b.Borrowers.Select(br => br.Name).Distinct().ToList()
    })
    .ToList();

foreach (var item in borrowersPerBook)
{
    var names = item.Borrowers.Count == 0 ? "(never borrowed)" : string.Join(", ", item.Borrowers);
    Console.WriteLine($"  {item.Title,-22} -> {names}");
}

// -----------------------------------------------------------------------------
// 4) MANY-TO-MANY using the join entity: we need LoanDate / ReturnDate, so we query Loan directly.
//    ReturnDate == null means the book is still out.
// -----------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== 4. Books currently on loan (join entity with extra data) ===");
const int loanPeriodDays = 14;
var overdueCutoff = today.AddDays(-loanPeriodDays);

var activeLoans = db.Loans
    .AsNoTracking()
    .Include(l => l.Book)
    .Include(l => l.Borrower)
    .Where(l => l.ReturnDate == null)
    .OrderBy(l => l.LoanDate)
    .ToList();

foreach (var loan in activeLoans)
{
    var status = loan.LoanDate < overdueCutoff ? "OVERDUE" : "on time";
    Console.WriteLine($"  {loan.Book.Title,-22} borrowed by {loan.Borrower.Name,-12} on {loan.LoanDate:yyyy-MM-dd}  [{status}]");
}

// -----------------------------------------------------------------------------
// 5) Aggregate across the relationship: how many times was each book borrowed?
// -----------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== 5. Loan counts per book ===");
var loanCounts = db.Books
    .AsNoTracking()
    .Select(b => new { b.Title, Count = b.Loans.Count })
    .OrderByDescending(x => x.Count)
    .ThenBy(x => x.Title)
    .ToList();

foreach (var item in loanCounts)
{
    Console.WriteLine($"  {item.Title,-22} {item.Count} loan(s)");
}

Console.WriteLine();
Console.WriteLine("Done. Open the LibrarySystemDb database in SSMS / Visual Studio to inspect the tables.");
