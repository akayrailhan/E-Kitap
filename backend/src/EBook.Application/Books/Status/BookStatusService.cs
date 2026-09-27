using EBook.Application.Abstractions.Persistence;

namespace EBook.Application.Books.Status;

public sealed class BookStatusService(IBookRepository bookRepository) : IBookStatusService
{
    public async Task<BookStatusResult?> GetAsync(
        Guid bookId,
        string ownerUserId,
        CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(bookId, ownerUserId, cancellationToken);
        if (book is null)
        {
            return null;
        }

        return new BookStatusResult(
            book.Id,
            book.Name,
            book.Status,
            book.ErrorMessage,
            book.PdfPath,
            book.Papers
                .OrderBy(paper => paper.Order)
                .Select(paper => new PaperStatusResult(
                    paper.Id,
                    paper.OriginalFileName,
                    paper.Title,
                    paper.Order))
                .ToArray());
    }
}