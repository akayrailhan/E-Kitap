using EBook.Application.Abstractions.Storage;

namespace EBook.Infrastructure.Storage;

public sealed class LocalFileStorage(string rootPath) : IFileStorage
{
    public async Task<string> SaveAsync(
        Guid bookId,
        int order,
        string originalFileName,
        Stream content,
        CancellationToken cancellationToken)
    {
        var safeFileName = Path.GetFileName(originalFileName);
        var relativeDirectory = Path.Combine("uploads", bookId.ToString("N"));
        var absoluteDirectory = Path.Combine(rootPath, bookId.ToString("N"));
        Directory.CreateDirectory(absoluteDirectory);

        var storedFileName = $"{order:D2}-{safeFileName}";
        var absolutePath = Path.Combine(absoluteDirectory, storedFileName);
        await using var output = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(output, cancellationToken);

        return Path.Combine(relativeDirectory, storedFileName).Replace(Path.DirectorySeparatorChar, '/');
    }

    public async Task<string> SavePdfAsync(Guid bookId, Stream content, CancellationToken cancellationToken)
    {
        var relativeDirectory = Path.Combine("uploads", bookId.ToString("N"));
        var absoluteDirectory = Path.Combine(rootPath, bookId.ToString("N"));
        Directory.CreateDirectory(absoluteDirectory);

        var absolutePath = Path.Combine(absoluteDirectory, "ebook.pdf");
        await using var output = new FileStream(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(output, cancellationToken);

        return Path.Combine(relativeDirectory, "ebook.pdf").Replace(Path.DirectorySeparatorChar, '/');
    }

    public Task DeleteBookAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(rootPath, bookId.ToString("N"));
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
