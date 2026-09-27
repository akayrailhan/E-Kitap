using EBook.Application.Documents;

namespace EBook.Application.Abstractions.Documents;

public interface IDocumentReader
{
    Task<DocumentContent> ReadAsync(Stream content, CancellationToken cancellationToken);
}