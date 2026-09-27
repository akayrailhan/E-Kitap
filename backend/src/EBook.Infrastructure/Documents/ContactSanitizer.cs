using System.Text.RegularExpressions;
using EBook.Application.Abstractions.Documents;
using EBook.Application.Documents;

namespace EBook.Infrastructure.Documents;

public sealed partial class ContactSanitizer : IContactSanitizer
{
    public DocumentContent Sanitize(DocumentContent content)
    {
        var sanitizedParagraphs = content.Paragraphs
            .Select(SanitizeParagraph)
            .ToArray();

        return new DocumentContent(sanitizedParagraphs);
    }

    private static string SanitizeParagraph(string paragraph)
    {
        var withoutEmails = EmailPattern().Replace(paragraph, string.Empty);
        return PhonePattern().Replace(withoutEmails, string.Empty);
    }

    [GeneratedRegex(@"(?<![\w.+-])[\w.!#$%&'*+/=?^`{|}~-]+@[\w](?:[\w-]{0,61}[\w])?(?:\.[\w](?:[\w-]{0,61}[\w])?)+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"(?<!\w)(?:\+?\d[\d\s().-]{7,}\d)(?!\w)", RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();
}