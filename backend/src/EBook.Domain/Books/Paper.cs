namespace EBook.Domain.Books;

public sealed class Paper
{
    private Paper()
    {
    }

    public Paper(string originalFileName, string storedFilePath, int order)
    {
        OriginalFileName = string.IsNullOrWhiteSpace(originalFileName)
            ? throw new ArgumentException("Original file name is required.", nameof(originalFileName))
            : originalFileName.Trim();
        StoredFilePath = string.IsNullOrWhiteSpace(storedFilePath)
            ? throw new ArgumentException("Stored file path is required.", nameof(storedFilePath))
            : storedFilePath.Trim();
        Order = order > 0 ? order : throw new ArgumentOutOfRangeException(nameof(order));
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BookId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFilePath { get; private set; } = string.Empty;
    public string? Title { get; private set; }
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Book Book { get; private set; } = null!;

    public void SetTitle(string title)
    {
        var resolvedTitle = string.IsNullOrWhiteSpace(title)
            ? Path.GetFileNameWithoutExtension(OriginalFileName)
            : title.Trim();

        if (resolvedTitle.Length > 500)
        {
            resolvedTitle = resolvedTitle[..500];
        }

        Title = resolvedTitle;
    }
}