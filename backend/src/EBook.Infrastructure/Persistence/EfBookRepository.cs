using EBook.Application.Abstractions.Persistence;
using EBook.Domain.Books;

namespace EBook.Infrastructure.Persistence;

public sealed class EfBookRepository(EBookDbContext dbContext) : IBookRepository
{
    public Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        return dbContext.Books.AddAsync(book, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
