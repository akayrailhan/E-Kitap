using EBook.Application.Abstractions.Documents;
using EBook.Application.Abstractions.Persistence;
using EBook.Application.Abstractions.Storage;
using EBook.Application.Books.Generation;
using EBook.Application.Documents;
using EBook.Domain.Books;
using EBook.Infrastructure.Documents;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EBook.Infrastructure.Tests.Documents;

public sealed class BookGenerationServiceTests
{
    [Fact]
    public async Task GeneratesBookSuccessfullyAndUpdatesStatusToCompleted()
    {
        var book = CreateBookWithTenPapers();
        var repo = new InMemoryBookRepository(book);
        var storage = new InMemoryFileStorage();
        var reader = new FakeDocumentReader();
        var sanitizer = new ContactSanitizer();
        var pdfGen = new FakePdfGenerator();
        var logger = NullLogger<BookGenerationService>.Instance;

        var service = new BookGenerationService(repo, storage, reader, sanitizer, pdfGen, logger);

        await service.GenerateBookAsync(book.Id, book.OwnerUserId, CancellationToken.None);

        Assert.Equal(BookStatus.Completed, book.Status);
        Assert.NotNull(book.PdfPath);
        Assert.Null(book.ErrorMessage);
        Assert.NotNull(book.CompletedAt);
        Assert.All(book.Papers, paper => Assert.False(string.IsNullOrWhiteSpace(paper.Title)));
    }

    [Fact]
    public async Task MarksBookAsFailedWhenReaderThrows()
    {
        var book = CreateBookWithTenPapers();
        var repo = new InMemoryBookRepository(book);
        var storage = new InMemoryFileStorage();
        var failingReader = new FailingDocumentReader();
        var sanitizer = new ContactSanitizer();
        var pdfGen = new FakePdfGenerator();
        var logger = NullLogger<BookGenerationService>.Instance;

        var service = new BookGenerationService(repo, storage, failingReader, sanitizer, pdfGen, logger);

        await service.GenerateBookAsync(book.Id, book.OwnerUserId, CancellationToken.None);

        Assert.Equal(BookStatus.Failed, book.Status);
        Assert.Contains("Corrupted docx", book.ErrorMessage);
        Assert.Null(book.PdfPath);
    }

    [Fact]
    public async Task StartGenerationThrowsWhenPaperCountIsNotTen()
    {
        var book = new Book("Eksik Bildirili Kitap", "user-1");
        book.Papers.Add(new Paper("p1.docx", "uploads/p1.docx", 1));

        var repo = new InMemoryBookRepository(book);
        var storage = new InMemoryFileStorage();
        var reader = new FakeDocumentReader();
        var sanitizer = new ContactSanitizer();
        var pdfGen = new FakePdfGenerator();
        var logger = NullLogger<BookGenerationService>.Instance;

        var service = new BookGenerationService(repo, storage, reader, sanitizer, pdfGen, logger);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartGenerationAsync(book.Id, book.OwnerUserId, CancellationToken.None));
    }

    [Fact]
    public async Task StartGenerationResetsFailedStatusAndAllowsSuccessfulRetry()
    {
        var book = CreateBookWithTenPapers();
        book.StartProcessing();
        book.MarkFailed("Önceki denemede hata oluştu");

        Assert.Equal(BookStatus.Failed, book.Status);
        Assert.NotNull(book.ErrorMessage);

        var repo = new InMemoryBookRepository(book);
        var storage = new InMemoryFileStorage();
        var reader = new FakeDocumentReader();
        var sanitizer = new ContactSanitizer();
        var pdfGen = new FakePdfGenerator();
        var logger = NullLogger<BookGenerationService>.Instance;

        var service = new BookGenerationService(repo, storage, reader, sanitizer, pdfGen, logger);

        var result = await service.StartGenerationAsync(book.Id, book.OwnerUserId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(BookStatus.Completed, book.Status);
        Assert.Null(book.ErrorMessage);
        Assert.NotNull(book.PdfPath);
        Assert.NotNull(book.CompletedAt);
    }

    private static Book CreateBookWithTenPapers()
    {
        var book = new Book("Akademik Bildiriler 2026", "user-1");
        for (var i = 1; i <= 10; i++)
        {
            book.Papers.Add(new Paper($"bildiri-{i:D2}.docx", $"uploads/test/bildiri-{i:D2}.docx", i));
        }
        return book;
    }

    private sealed class InMemoryBookRepository(Book initialBook) : IBookRepository
    {
        private readonly Book _book = initialBook;

        public Task<Book?> GetByIdAsync(Guid bookId, CancellationToken cancellationToken)
        {
            return Task.FromResult<Book?>(_book.Id == bookId ? _book : null);
        }

        public Task<Book?> GetByIdAsync(Guid bookId, string ownerUserId, CancellationToken cancellationToken)
        {
            return Task.FromResult<Book?>(_book.Id == bookId && _book.OwnerUserId == ownerUserId ? _book : null);
        }

        public Task<IReadOnlyList<Book>> ListByOwnerUserIdAsync(string ownerUserId, CancellationToken cancellationToken)
        {
            IReadOnlyList<Book> list = _book.OwnerUserId == ownerUserId ? [_book] : [];
            return Task.FromResult(list);
        }

        public Task AddAsync(Book book, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryFileStorage : IFileStorage
    {
        public Task<string> SaveAsync(Guid bookId, int order, string originalFileName, Stream content, CancellationToken cancellationToken)
            => Task.FromResult($"uploads/{bookId}/{order:D2}-{originalFileName}");

        public Task<string> SavePdfAsync(Guid bookId, Stream content, CancellationToken cancellationToken)
            => Task.FromResult($"uploads/{bookId}/ebook.pdf");

        public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
            => Task.FromResult<Stream>(new MemoryStream([1, 2, 3, 4]));

        public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken)
            => Task.FromResult(true);

        public Task DeleteBookAsync(Guid bookId, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class FakeDocumentReader : IDocumentReader
    {
        public Task<DocumentContent> ReadAsync(Stream content, CancellationToken cancellationToken)
        {
            return Task.FromResult(new DocumentContent([
                "Yapay Zeka Çalışması",
                "İletişim: test@akap.tr 0312 343 10 33",
                "Bu çalışma yapay zeka modelinin sonuçlarını içermektedir."
            ]));
        }
    }

    private sealed class FailingDocumentReader : IDocumentReader
    {
        public Task<DocumentContent> ReadAsync(Stream content, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Corrupted docx file.");
        }
    }

    private sealed class FakePdfGenerator : IBookPdfGenerator
    {
        public Task GenerateAsync(PdfBookContent content, Stream output, CancellationToken cancellationToken)
        {
            var bytes = "%PDF-1.4 Fake PDF Content"u8.ToArray();
            output.Write(bytes, 0, bytes.Length);
            return Task.CompletedTask;
        }
    }
}
