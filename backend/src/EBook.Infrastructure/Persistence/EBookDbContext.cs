using EBook.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace EBook.Infrastructure.Persistence;

public sealed class EBookDbContext(DbContextOptions<EBookDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Paper> Papers => Set<Paper>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EBookDbContext).Assembly);
    }
}