using EBook.Application.Books.Status;

namespace EBook.Application.Books.Generation;

public interface IBookGenerationService
{
    Task<BookStatusResult?> StartGenerationAsync(Guid bookId, string ownerUserId, CancellationToken cancellationToken);

    Task GenerateBookAsync(Guid bookId, string ownerUserId, CancellationToken cancellationToken);
}
