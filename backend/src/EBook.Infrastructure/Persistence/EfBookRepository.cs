using EBook.Application.Abstractions.Persistence;
using EBook.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace EBook.Infrastructure.Persistence;

public sealed class EfBookRepository(EBookDbContext dbContext) : IBookRepository
{
    public Task<Book?> GetByIdAsync(Guid bookId, string ownerUserId, CancellationToken cancellationToken)
    {
        return dbContext.Books
            .Include(book => book.Papers)
            .SingleOrDefaultAsync(
                book => book.Id == bookId && book.OwnerUserId == ownerUserId,
                cancellationToken);
    }

    public Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        return dbContext.Books.AddAsync(book, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
