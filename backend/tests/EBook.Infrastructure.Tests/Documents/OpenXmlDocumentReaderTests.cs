using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EBook.Infrastructure.Documents;
using Xunit;

namespace EBook.Infrastructure.Tests.Documents;

public sealed class OpenXmlDocumentReaderTests
{
    [Fact]
    public async Task ReadsParagraphsFromDocxStream()
    {
        await using var documentStream = CreateDocument("Başlık", "İçerik paragrafı");
        var reader = new OpenXmlDocumentReader();

        var result = await reader.ReadAsync(documentStream, CancellationToken.None);

        Assert.Equal(["Başlık", "İçerik paragrafı"], result.Paragraphs);
    }

    private static MemoryStream CreateDocument(params string[] paragraphs)
    {
        var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(
                   stream,
                   DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
                   true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document(new Body(
                paragraphs.Select(text => new Paragraph(new Run(new Text(text))))));
            mainPart.Document.Save();
        }

        stream.Position = 0;
        return stream;
    }
}