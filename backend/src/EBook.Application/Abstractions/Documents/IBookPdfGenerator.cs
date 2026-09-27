namespace EBook.Application.Abstractions.Documents;

public interface IBookPdfGenerator
{
    Task GenerateAsync(
        PdfBookContent content,
        Stream output,
        CancellationToken cancellationToken);
}

public sealed record PdfBookContent(
    string BookName,
    IReadOnlyList<PdfPaperContent> Papers);

public sealed record PdfPaperContent(
    string SectionId,
    string Title,
    IReadOnlyList<string> Paragraphs);