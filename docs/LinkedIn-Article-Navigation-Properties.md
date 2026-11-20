# Navigation Properties in EF Core: Let Your Objects Do the Joining

If you have ever written a SQL JOIN by hand and then wished your C# objects could "just know" how they relate, that is exactly what **navigation properties** in Entity Framework Core give you.

## What is a navigation property?

A navigation property is a property on an entity class that points to another entity (or a collection of them). It does **not** create a column. It lets you move across a relationship in C# the same way a JOIN does in SQL.

Two flavours:

- **Reference navigation**: points to one related entity, e.g. `book.Author`
- **Collection navigation**: points to many, e.g. `author.Books`

Behind them sits the **foreign key property** (e.g. `Book.AuthorId`), which is the real column in the database.

## Example: a small library system

I modelled a library with four entities: `Author`, `Book`, `Borrower` and `Loan`.

### One-to-Many: Author to Book

```csharp
public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();   // collection navigation
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }                                  // foreign key
    public Author Author { get; set; } = null!;                        // reference navigation
}
```

The "one" side holds the collection, the "many" side holds the foreign key plus a reference back. EF Core discovers this by naming convention, and I can state it explicitly with the Fluent API:

```csharp
entity.HasOne(b => b.Author)
      .WithMany(a => a.Books)
      .HasForeignKey(b => b.AuthorId);
```

### Many-to-Many with data: Book to Borrower through Loan

A book can be borrowed by many people, and a person can borrow many books. A loan also has its own data (`LoanDate`, `ReturnDate`), so the join table becomes a real entity:

```csharp
public class Loan
{
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int BorrowerId { get; set; }
    public Borrower Borrower { get; set; } = null!;
    public DateOnly LoanDate { get; set; }
    public DateOnly? ReturnDate { get; set; }   // null = still on loan
}
```

EF Core also supports **skip navigations** (`Book.Borrowers`, `Borrower.Books`) that jump over the join entity when you only care about who, not when:

```csharp
modelBuilder.Entity<Book>()
    .HasMany(b => b.Borrowers)
    .WithMany(br => br.Books)
    .UsingEntity<Loan>(/* configure both sides and the composite key */);
```

## Why it matters

Compare the two styles:

```csharp
// With navigation properties
var titles = db.Authors
    .Where(a => a.Name == "George Orwell")
    .SelectMany(a => a.Books)
    .Select(b => b.Title)
    .ToList();
```

No manual JOIN, no ON clause. EF Core translates it to SQL for you.

## Practical tips I learned

1. **Use `Include()`** when you need related data loaded; otherwise navigation properties stay empty.
2. **Prefer projections (`Select`)** when you only need a few columns.
3. **Choose delete behaviour on purpose.** The default for a required relationship is cascade delete, which can remove far more than you expect. I used `Restrict` to protect catalogue and loan history.
4. **Think about the join entity key.** A key of `(BookId, BorrowerId)` alone would stop a member from ever borrowing the same book twice, so I included `LoanDate`.
5. **Initialise collections** to an empty list so you never hit a null reference when adding items.

## Takeaway

Navigation properties turn database relationships into something you can read like normal C# code. Define the keys, define the navigations, describe the relationship once, and let EF Core handle the SQL.

*Full source code and ERD: [add your GitHub link here]*

#dotnet #csharp #efcore #entityframework #sqlserver #softwareengineering
