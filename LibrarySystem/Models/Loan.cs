namespace LibrarySystem.Models;

/// <summary>
/// JOIN ENTITY between Book and Borrower.
///
/// Why a class and not a hidden table?
/// A plain many-to-many only needs two foreign keys. But a loan has its own data
/// (LoanDate, ReturnDate), so we model the join table explicitly as an entity ("payload" join).
///
/// Primary key: composite (BookId, BorrowerId, LoanDate) - configured in LibraryContext.
/// Why include LoanDate? If the key were only (BookId, BorrowerId), a member could borrow a given
/// book just ONCE in their whole life. Adding LoanDate allows repeat loans on different days
/// while still preventing exact duplicates.
/// </summary>
public class Loan
{
    // ---- Foreign key to Book (and its reference navigation) ----
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    // ---- Foreign key to Borrower (and its reference navigation) ----
    public int BorrowerId { get; set; }
    public Borrower Borrower { get; set; } = null!;

    /// <summary>The day the book left the library (part of the composite key).</summary>
    public DateOnly LoanDate { get; set; }

    /// <summary>
    /// The day the book came back. Nullable (DateOnly?) because while the book is still out
    /// there is no return date yet: NULL means "currently on loan".
    /// </summary>
    public DateOnly? ReturnDate { get; set; }
}
