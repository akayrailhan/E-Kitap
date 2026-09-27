namespace EBook.Application.Abstractions.Storage;

public interface IFileStorage
{
    Task<string> SaveAsync(Guid bookId, int order, string originalFileName, Stream content, CancellationToken cancellationToken);

    Task DeleteBookAsync(Guid bookId, CancellationToken cancellationToken);
}
