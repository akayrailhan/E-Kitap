using EBook.Application.Documents;
using EBook.Infrastructure.Documents;
using Xunit;

namespace EBook.Infrastructure.Tests.Documents;

public sealed class ContactSanitizerTests
{
    [Fact]
    public void RemovesEmailAndPhoneButKeepsSurroundingText()
    {
        var sanitizer = new ContactSanitizer();
        var content = new DocumentContent([
            "İletişim: author@example.org, +90 (555) 123-45-67.",
            "Yıl: 2026"
        ]);

        var result = sanitizer.Sanitize(content);

        Assert.DoesNotContain("author@example.org", result.Paragraphs[0]);
        Assert.DoesNotContain("555", result.Paragraphs[0]);
        Assert.Contains("İletişim:", result.Paragraphs[0]);
        Assert.Equal("Yıl: 2026", result.Paragraphs[1]);
    }

    [Fact]
    public void KeepsOrdinaryTextWithoutContacts()
    {
        var sanitizer = new ContactSanitizer();
        var content = new DocumentContent(["Bu paragraf iletişim bilgisi içermez."]);

        var result = sanitizer.Sanitize(content);

        Assert.Equal(content.Paragraphs, result.Paragraphs);
    }
}