namespace EBook.Domain.Books;

public sealed class Book
{
    private Book()
    {
    }

    public Book(string name, string ownerUserId)
    {
        Rename(name);
        OwnerUserId = string.IsNullOrWhiteSpace(ownerUserId)
            ? throw new ArgumentException("Owner user id is required.", nameof(ownerUserId))
            : ownerUserId;
        CreatedAt = DateTimeOffset.UtcNow;
        Status = BookStatus.Draft;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string OwnerUserId { get; private set; } = string.Empty;
    public BookStatus Status { get; private set; }
    public string? PdfPath { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public ICollection<Paper> Papers { get; private set; } = new List<Paper>();

    public void Rename(string name)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Book name is required.", nameof(name))
            : name.Trim();
    }

    public void StartProcessing()
    {
        if (Papers.Count != 10)
        {
            throw new InvalidOperationException("A book must contain exactly 10 papers before processing.");
        }

        Status = BookStatus.Processing;
        ErrorMessage = null;
        CompletedAt = null;
    }

    public void MarkCompleted(string pdfPath)
    {
        PdfPath = string.IsNullOrWhiteSpace(pdfPath)
            ? throw new ArgumentException("PDF path is required.", nameof(pdfPath))
            : pdfPath;
        Status = BookStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = BookStatus.Failed;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Book generation failed." : errorMessage.Trim();
    }
}