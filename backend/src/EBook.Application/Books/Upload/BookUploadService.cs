using EBook.Application.Abstractions.Persistence;
using EBook.Application.Abstractions.Storage;
using EBook.Domain.Books;

namespace EBook.Application.Books.Upload;

public sealed class BookUploadService(
    IBookRepository bookRepository,
    IFileStorage fileStorage) : IBookUploadService
{
    private const int RequiredPaperCount = 10;
    private const long MaximumPaperSize = 10 * 1024 * 1024;

    public async Task<BookUploadResult> UploadAsync(
        string bookName,
        string ownerUserId,
        IReadOnlyList<UploadedPaper> papers,
        CancellationToken cancellationToken)
    {
        if (papers.Count != RequiredPaperCount)
        {
            throw new ArgumentException("Exactly 10 DOCX files are required.", nameof(papers));
        }

        if (papers.Any(paper => !Path.GetExtension(paper.OriginalFileName).Equals(".docx", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Only .docx files are supported.", nameof(papers));
        }

        if (papers.Any(paper => paper.Content.Length > MaximumPaperSize))
        {
            throw new ArgumentException("Each DOCX file must be 10 MB or smaller.", nameof(papers));
        }

        var book = new Book(bookName, ownerUserId);

        try
        {
            for (var index = 0; index < papers.Count; index++)
            {
                var paper = papers[index];
                var order = index + 1;
                var storedPath = await fileStorage.SaveAsync(
                    book.Id,
                    order,
                    paper.OriginalFileName,
                    paper.Content,
                    cancellationToken);

                book.Papers.Add(new Paper(paper.OriginalFileName, storedPath, order));
            }

            await bookRepository.AddAsync(book, cancellationToken);
            await bookRepository.SaveChangesAsync(cancellationToken);

            return new BookUploadResult(
                book.Id,
                book.Name,
                book.Papers
                    .OrderBy(paper => paper.Order)
                    .Select(paper => new PaperUploadResult(paper.Id, paper.OriginalFileName, paper.Order))
                    .ToArray());
        }
        catch
        {
            await fileStorage.DeleteBookAsync(book.Id, CancellationToken.None);
            throw;
        }
    }
}
