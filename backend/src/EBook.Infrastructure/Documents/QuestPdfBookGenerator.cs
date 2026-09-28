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
            // Cover page
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(48);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Content().AlignCenter().AlignMiddle().Column(cover =>
                {
                    cover.Item().Text(content.BookName).FontSize(28).Bold().FontColor("#333");
                    cover.Item().PaddingTop(14).Text("Bildiri Kitabi").FontSize(14).FontColor("#666");
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                });
            });

            // Table of contents page
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(48);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Content().Column(column =>
                {
                    column.Item().Text("Icindekiler").FontSize(22).Bold();
                    column.Item().PaddingTop(16).Column(toc =>
                    {
                        foreach (var paper in content.Papers)
                        {
                            toc.Item().PaddingBottom(6).Row(row =>
                            {
                                row.RelativeItem().SectionLink(paper.SectionId)
                                    .Text(paper.Title).FontSize(11);
                                row.ConstantItem(40).AlignRight()
                                    .Text(text => text.BeginPageNumberOfSection(paper.SectionId));
                            });
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                });
            });

            // Paper pages
            foreach (var paper in content.Papers)
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(48);
                    page.DefaultTextStyle(style => style.FontSize(10));

                    page.Content().Section(paper.SectionId).Column(section =>
                    {
                        section.Item().Text(paper.Title).FontSize(18).Bold();
                        section.Item().PaddingTop(12).LineHorizontal(1).LineColor("#ccc");
                        foreach (var paragraph in paper.Paragraphs)
                        {
                            if (!string.IsNullOrWhiteSpace(paragraph))
                            {
                                section.Item().PaddingTop(8).Text(paragraph).FontSize(10).LineHeight(1.4f);
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Sayfa ");
                        text.CurrentPageNumber();
                    });
                });
            }
        }).GeneratePdf(output);

        return Task.CompletedTask;
    }
}