using EBook.Application.Abstractions.Documents;
using EBook.Application.Abstractions.Persistence;
using EBook.Application.Abstractions.Storage;
using EBook.Application.Books.Status;
using EBook.Domain.Books;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EBook.Application.Books.Generation;

public sealed class BookGenerationService(
    IBookRepository bookRepository,
    IFileStorage fileStorage,
    IDocumentReader documentReader,
    IContactSanitizer contactSanitizer,
    IBookPdfGenerator pdfGenerator,
    ILogger<BookGenerationService> logger,
    IServiceScopeFactory? serviceScopeFactory = null) : IBookGenerationService
{
    private const int RequiredPaperCount = 10;
    private const int MaximumTitleLength = 200;

    public async Task<BookStatusResult?> StartGenerationAsync(
        Guid bookId,
        string ownerUserId,
        CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(bookId, ownerUserId, cancellationToken);
        if (book is null)
        {
            return null;
        }

        if (book.Papers.Count != RequiredPaperCount)
        {
            throw new InvalidOperationException($"A book must contain exactly {RequiredPaperCount} papers before processing.");
        }

        if (book.Status == BookStatus.Processing)
        {
            return MapToStatusResult(book);
        }

        book.StartProcessing();
        await bookRepository.SaveChangesAsync(cancellationToken);

        if (serviceScopeFactory is not null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var scopedService = scope.ServiceProvider.GetRequiredService<IBookGenerationService>();
                    await scopedService.GenerateBookAsync(bookId, ownerUserId, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Background generation failed unexpectedly for Book {BookId}", bookId);
                }
            });
        }
        else
        {
            await GenerateBookAsync(bookId, ownerUserId, cancellationToken);
        }

        return MapToStatusResult(book);
    }

    public async Task GenerateBookAsync(
        Guid bookId,
        string ownerUserId,
        CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(bookId, ownerUserId, cancellationToken);
        if (book is null)
        {
            logger.LogWarning("Book {BookId} for user {UserId} not found for generation.", bookId, ownerUserId);
            return;
        }

        try
        {
            if (book.Papers.Count != RequiredPaperCount)
            {
                throw new InvalidOperationException($"A book must contain exactly {RequiredPaperCount} papers before processing.");
            }

            var orderedPapers = book.Papers.OrderBy(paper => paper.Order).ToList();
            var pdfPapers = new List<PdfPaperContent>(orderedPapers.Count);

            foreach (var paper in orderedPapers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using var docxStream = await fileStorage.OpenReadAsync(paper.StoredFilePath, cancellationToken);
                var rawContent = await documentReader.ReadAsync(docxStream, cancellationToken);
                var sanitizedContent = contactSanitizer.Sanitize(rawContent);

                var nonBlankParagraphs = sanitizedContent.Paragraphs
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim())
                    .ToList();

                string title;
                IReadOnlyList<string> bodyParagraphs;

                if (nonBlankParagraphs.Count > 0 && nonBlankParagraphs[0].Length <= MaximumTitleLength)
                {
                    title = nonBlankParagraphs[0];
                    bodyParagraphs = nonBlankParagraphs.Skip(1).ToList();
                    if (bodyParagraphs.Count == 0)
                    {
                        bodyParagraphs = nonBlankParagraphs;
                    }
                }
                else
                {
                    title = Path.GetFileNameWithoutExtension(paper.OriginalFileName);
                    bodyParagraphs = nonBlankParagraphs;
                }

                paper.SetTitle(title);
                pdfPapers.Add(new PdfPaperContent($"paper-{paper.Order}", title, bodyParagraphs));
            }

            var pdfContent = new PdfBookContent(book.Name, pdfPapers);
            using var pdfStream = new MemoryStream();
            await pdfGenerator.GenerateAsync(pdfContent, pdfStream, cancellationToken);
            pdfStream.Position = 0;

            var pdfPath = await fileStorage.SavePdfAsync(book.Id, pdfStream, cancellationToken);
            book.MarkCompleted(pdfPath);
            await bookRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to generate e-book for book {BookId}", bookId);
            book.MarkFailed(exception.Message);

            try
            {
                await bookRepository.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception dbException)
            {
                logger.LogError(dbException, "Failed to save failed status to database for book {BookId}", bookId);
            }
        }
    }

    private static BookStatusResult MapToStatusResult(Book book)
    {
        return new BookStatusResult(
            book.Id,
            book.Name,
            book.Status,
            book.ErrorMessage,
            book.PdfPath,
            book.Papers
                .OrderBy(paper => paper.Order)
                .Select(paper => new PaperStatusResult(
                    paper.Id,
                    paper.OriginalFileName,
                    paper.Title,
                    paper.Order))
                .ToArray());
    }
}
