using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data;

/// <summary>
/// The DbContext is EF Core's "session" with the database:
///   * each DbSet&lt;T&gt; represents a table,
///   * OnModelCreating describes how our classes map to tables, keys and relationships.
/// </summary>
public class LibraryContext : DbContext
{
    // The options (which provider, which connection string) are passed in from outside.
    // This keeps the context free of hard-coded settings and easy to test.
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
    {
    }

    // One DbSet per entity => one table each.
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Borrower> Borrowers => Set<Borrower>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================================
        // AUTHOR
        // =====================================================================
        modelBuilder.Entity<Author>(entity =>
        {
            // Without MaxLength a string becomes nvarchar(max), which cannot be indexed efficiently.
            entity.Property(a => a.Name)
                  .IsRequired()
                  .HasMaxLength(150);
        });

        // =====================================================================
        // BOOK
        // =====================================================================
        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(b => b.Title)
                  .IsRequired()
                  .HasMaxLength(250);

            // ISBN-13 with hyphens is at most 17 characters; 20 leaves some slack.
            entity.Property(b => b.ISBN)
                  .IsRequired()
                  .HasMaxLength(20);

            // A unique index = the database itself rejects duplicate ISBNs.
            entity.HasIndex(b => b.ISBN).IsUnique();

            // ---------------------------------------------------------------
            // ONE-TO-MANY: Author (1) -> Books (many)
            // Read it as: "A Book has one Author; an Author has many Books;
            //              the foreign key is Book.AuthorId."
            // ---------------------------------------------------------------
            entity.HasOne(b => b.Author)
                  .WithMany(a => a.Books)
                  .HasForeignKey(b => b.AuthorId)
                  // Restrict = refuse to delete an Author who still has Books.
                  // (The default for a required relationship is Cascade, which would silently
                  //  delete the author's whole catalogue - too dangerous for library data.)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // BORROWER
        // =====================================================================
        modelBuilder.Entity<Borrower>(entity =>
        {
            entity.Property(b => b.Name)
                  .IsRequired()
                  .HasMaxLength(150);
        });

        // =====================================================================
        // MANY-TO-MANY: Book <-> Borrower, through the explicit Loan entity
        // =====================================================================
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Borrowers)          // skip navigation on Book
            .WithMany(br => br.Books)           // skip navigation on Borrower
            .UsingEntity<Loan>(
                // NOTE the order: the FIRST lambda configures the relationship to the RIGHT side
                // (Borrower), the SECOND configures the LEFT side (Book).

                // Borrower (1) -> Loans (many), FK = Loan.BorrowerId
                right => right
                    .HasOne(l => l.Borrower)
                    .WithMany(br => br.Loans)
                    .HasForeignKey(l => l.BorrowerId)
                    // Restrict: deleting a borrower must not erase the loan history.
                    .OnDelete(DeleteBehavior.Restrict),

                // Book (1) -> Loans (many), FK = Loan.BookId
                left => left
                    .HasOne(l => l.Book)
                    .WithMany(b => b.Loans)
                    .HasForeignKey(l => l.BookId)
                    .OnDelete(DeleteBehavior.Restrict),

                // Settings for the Loan table itself.
                join =>
                {
                    // Composite primary key: see the comment on the Loan class for why LoanDate is included.
                    join.HasKey(l => new { l.BookId, l.BorrowerId, l.LoanDate });

                    // A book cannot come back before it left. NULL = still on loan, which is allowed.
                    join.ToTable("Loans", t =>
                        t.HasCheckConstraint(
                            "CK_Loans_ReturnDate_After_LoanDate",
                            "[ReturnDate] IS NULL OR [ReturnDate] >= [LoanDate]"));

                    // Speeds up "which books does this borrower have?" lookups.
                    // (The PK index already covers lookups that start with BookId.)
                    join.HasIndex(l => l.BorrowerId);
                });
    }
}
