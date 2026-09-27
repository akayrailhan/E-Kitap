using EBook.Domain.Books;

namespace EBook.Application.Abstractions.Persistence;

public interface IBookRepository
{
    Task AddAsync(Book book, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
