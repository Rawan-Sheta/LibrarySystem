namespace LibrarySystem.Models;

/// <summary>
/// A library member who can borrow books.
///
/// Relationship: MANY Borrowers <-> MANY Books (Many-to-Many) through the Loan join entity.
/// A borrower can borrow many books over time, and a book can be borrowed by many borrowers.
/// </summary>
public class Borrower
{
    /// <summary>Primary key (identity column).</summary>
    public int Id { get; set; }

    /// <summary>Member's full name (required).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The day this person joined the library (date only, no time).</summary>
    public DateOnly MembershipDate { get; set; }

    /// <summary>The Loan rows for this borrower (carries LoanDate / ReturnDate).</summary>
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    /// <summary>Skip navigation: every Book this borrower has ever borrowed, without touching Loan in C#.</summary>
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
