using EBook.Domain.Books;

namespace EBook.Application.Books.Status;

public interface IBookStatusService
{
    Task<BookStatusResult?> GetAsync(
        Guid bookId,
        string ownerUserId,
        CancellationToken cancellationToken);
}

/// <summary>Represents the current book generation state and its ordered papers.</summary>
public sealed record BookStatusResult(
    Guid BookId,
    string Name,
    BookStatus Status,
    string? ErrorMessage,
    string? PdfPath,
    IReadOnlyList<PaperStatusResult> Papers);

/// <summary>Represents a paper included in a book status response.</summary>
public sealed record PaperStatusResult(
    Guid PaperId,
    string OriginalFileName,
    string? Title,
    int Order);

/// <summary>Represents a book in the user's book history list.</summary>
public sealed record BookListItemResult(
    Guid BookId,
    string Name,
    BookStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    string? PdfPath,
    int PaperCount,
    string? ErrorMessage = null);