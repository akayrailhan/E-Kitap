using EBook.Domain.Books;

namespace EBook.Application.Abstractions.Persistence;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid bookId, string ownerUserId, CancellationToken cancellationToken);

    Task AddAsync(Book book, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
