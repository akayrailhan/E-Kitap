namespace EBook.Application.Documents;

public sealed record DocumentContent(IReadOnlyList<string> Paragraphs)
{
    public string ToPlainText() => string.Join(Environment.NewLine, Paragraphs);
}