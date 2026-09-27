using EBook.Application.Abstractions.Documents;
using EBook.Infrastructure.Documents;
using QuestPDF.Infrastructure;
using Xunit;

namespace EBook.Infrastructure.Tests.Documents;

public sealed class QuestPdfBookGeneratorTests
{
    [Fact]
    public async Task GeneratesPdfWithBookAndPaperContent()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        await using var output = new MemoryStream();
        var generator = new QuestPdfBookGenerator();
        var content = new PdfBookContent(
            "Test E-Kitap",
            [new PdfPaperContent("paper-1", "İlk Bildiri", ["İletişim bilgileri temizlenmiş metin."])]);

        await generator.GenerateAsync(content, output, CancellationToken.None);

        Assert.True(output.Length > 4);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(output.GetBuffer(), 0, 4));
    }
}