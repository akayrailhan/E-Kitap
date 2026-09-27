using EBook.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBook.Infrastructure.Persistence.Configurations;

public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(book => book.Id);
        builder.Property(book => book.Name).HasMaxLength(200).IsRequired();
        builder.Property(book => book.OwnerUserId).HasMaxLength(200).IsRequired();
        builder.Property(book => book.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(book => book.PdfPath).HasMaxLength(500);
        builder.Property(book => book.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(book => new { book.OwnerUserId, book.CreatedAt });
        builder.HasMany(book => book.Papers).WithOne(paper => paper.Book).HasForeignKey(paper => paper.BookId);
    }
}