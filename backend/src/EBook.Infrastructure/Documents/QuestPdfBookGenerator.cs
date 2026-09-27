using EBook.Application.Abstractions.Documents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EBook.Infrastructure.Documents;

public sealed class QuestPdfBookGenerator : IBookPdfGenerator
{
    public Task GenerateAsync(
        PdfBookContent content,
        Stream output,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(48);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Content().Column(column =>
                {
                    column.Item().AlignCenter().PaddingTop(170).Column(cover =>
                    {
                        cover.Item().Text(content.BookName).FontSize(30).Bold();
                        cover.Item().PaddingTop(18).Text("Bildiri kitabı").FontSize(14);
                    });

                    column.Item().PageBreak();
                    column.Item().Text("İçindekiler").FontSize(24).Bold();
                    column.Item().PaddingTop(18).Column(tableOfContents =>
                    {
                        foreach (var paper in content.Papers)
                        {
                            tableOfContents.Item().Row(row =>
                            {
                                row.AutoItem().SectionLink(paper.SectionId).Text(paper.Title);
                                row.RelativeItem().PaddingHorizontal(8).AlignBottom().LineHorizontal(1);
                                row.AutoItem().Text(text => text.BeginPageNumberOfSection(paper.SectionId));
                            });
                        }
                    });

                    foreach (var paper in content.Papers)
                    {
                        column.Item().PageBreak();
                        column.Item().Section(paper.SectionId).Column(section =>
                        {
                            section.Item().Text(paper.Title).FontSize(19).Bold();
                            foreach (var paragraph in paper.Paragraphs)
                            {
                                if (!string.IsNullOrWhiteSpace(paragraph))
                                {
                                    section.Item().PaddingTop(8).Text(paragraph);
                                }
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Sayfa ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf(output);

        return Task.CompletedTask;
    }
}