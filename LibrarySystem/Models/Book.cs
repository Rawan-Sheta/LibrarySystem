namespace LibrarySystem.Models;

/// <summary>
/// A book in the library catalogue.
///
/// Relationships:
///   * MANY Books -> ONE Author            (Many-to-One)  via AuthorId / Author
///   * MANY Books <-> MANY Borrowers       (Many-to-Many) through the Loan join entity
/// </summary>
public class Book
{
    /// <summary>Primary key (identity column).</summary>
    public int Id { get; set; }

    /// <summary>Book title (required).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// International Standard Book Number. We make it unique in the DbContext
    /// (two catalogue rows must not share an ISBN).
    /// </summary>
    public string ISBN { get; set; } = string.Empty;

    // ---------------------------------------------------------------
    // Many-to-One: Book -> Author
    // ---------------------------------------------------------------

    /// <summary>
    /// Foreign key column. Because it is a plain 'int' (not 'int?') the relationship is REQUIRED:
    /// every Book must have an Author.
    /// </summary>
    public int AuthorId { get; set; }

    /// <summary>
    /// Reference navigation property: lets us write book.Author.Name instead of manually joining.
    /// EF Core pairs it with AuthorId by naming convention ("Author" + "Id").
    /// 'null!' tells the compiler "EF Core will fill this in", silencing the nullable warning.
    /// </summary>
    public Author Author { get; set; } = null!;

    // ---------------------------------------------------------------
    // Many-to-Many: Book <-> Borrower, through Loan
    // ---------------------------------------------------------------

    /// <summary>
    /// The join-entity rows for this book (one Loan per time it was borrowed).
    /// Use this when you need the EXTRA data stored on the join (LoanDate, ReturnDate).
    /// </summary>
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    /// <summary>
    /// "Skip navigation": jumps straight over the Loan table to the Borrowers.
    /// Use this when you only care WHO borrowed the book, not WHEN.
    /// EF Core keeps Loans and Borrowers in sync automatically.
    /// </summary>
    public ICollection<Borrower> Borrowers { get; set; } = new List<Borrower>();
}
