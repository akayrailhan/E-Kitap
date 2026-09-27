namespace EBook.Application.Books.Upload;

public interface IBookUploadService
{
    Task<BookUploadResult> UploadAsync(
        string bookName,
        string ownerUserId,
        IReadOnlyList<UploadedPaper> papers,
        CancellationToken cancellationToken);
}

public sealed record BookUploadResult(Guid BookId, string Name, IReadOnlyList<PaperUploadResult> Papers);

public sealed record PaperUploadResult(Guid PaperId, string OriginalFileName, int Order);
