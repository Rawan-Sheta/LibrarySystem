namespace LibrarySystem.Models;

/// <summary>
/// An author who has written one or more books.
///
/// Relationship: ONE Author  ->  MANY Books   (One-to-Many)
/// This class is the "one" side, so it holds a COLLECTION navigation property (Books).
/// The foreign key itself lives on the "many" side (Book.AuthorId).
/// </summary>
public class Author
{
    /// <summary>Primary key. EF Core sees a property called "Id" and treats it as the PK by convention.
    /// For an int key on SQL Server it is created as IDENTITY (auto-increment).</summary>
    public int Id { get; set; }

    /// <summary>Author's full name. Non-nullable string => the column is NOT NULL.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth. DateOnly maps to the SQL "date" type (no time part), which is exactly
    /// what a birth date is. (Requires EF Core 8+ for SQL Server.)
    /// </summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Collection navigation property: every book written by this author.
    /// Initialised to an empty list so it is never null and we can call .Add(...) safely.
    /// It does NOT create a column in the Authors table; it only lets us walk the relationship in C#.
    /// </summary>
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
