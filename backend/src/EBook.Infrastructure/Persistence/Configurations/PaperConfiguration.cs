using EBook.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBook.Infrastructure.Persistence.Configurations;

public sealed class PaperConfiguration : IEntityTypeConfiguration<Paper>
{
    public void Configure(EntityTypeBuilder<Paper> builder)
    {
        builder.ToTable("Papers");
        builder.HasKey(paper => paper.Id);
        builder.Property(paper => paper.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(paper => paper.StoredFilePath).HasMaxLength(500).IsRequired();
        builder.Property(paper => paper.Title).HasMaxLength(500);
        builder.Property(paper => paper.Order).IsRequired();
        builder.HasIndex(paper => new { paper.BookId, paper.Order }).IsUnique();
    }
}