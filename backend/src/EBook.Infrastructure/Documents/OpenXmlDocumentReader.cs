using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EBook.Application.Abstractions.Documents;
using EBook.Application.Documents;

namespace EBook.Infrastructure.Documents;

public sealed class OpenXmlDocumentReader : IDocumentReader
{
    public async Task<DocumentContent> ReadAsync(Stream content, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();

        using var document = WordprocessingDocument.Open(content, false);
        var paragraphs = document.MainDocumentPart?.Document.Body?
            .Elements<Paragraph>()
            .Select(paragraph => paragraph.InnerText)
            .ToArray()
            ?? [];

        return new DocumentContent(paragraphs);
    }
}