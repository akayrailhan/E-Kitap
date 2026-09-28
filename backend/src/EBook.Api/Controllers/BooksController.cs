using System.Security.Claims;
using EBook.Application.Abstractions.Persistence;
using EBook.Application.Abstractions.Storage;
using EBook.Application.Books.Generation;
using EBook.Application.Books.Status;
using EBook.Application.Books.Upload;
using EBook.Domain.Books;
using Microsoft.AspNetCore.Mvc;

namespace EBook.Api.Controllers;

[ApiController]
[Route("api/books")]
public sealed class BooksController(
    IBookUploadService bookUploadService,
    IBookStatusService bookStatusService,
    IBookGenerationService bookGenerationService,
    IBookRepository bookRepository,
    IFileStorage fileStorage) : ControllerBase
{
    /// <summary>Lists all books belonging to the current user.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookListItemResult>>> List(CancellationToken cancellationToken)
    {
        var ownerUserId = GetOwnerUserId();
        var books = await bookRepository.ListByOwnerUserIdAsync(ownerUserId, cancellationToken);
        var results = books.Select(book => new BookListItemResult(
            book.Id,
            book.Name,
            book.Status,
            book.CreatedAt,
            book.CompletedAt,
            book.PdfPath,
            book.Papers.Count,
            book.ErrorMessage
        )).ToList();

        return Ok(results);
    }

    /// <summary>Returns the current generation state and ordered papers for a book.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookStatusResult>> GetStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var ownerUserId = GetOwnerUserId();
        var result = await bookStatusService.GetAsync(id, ownerUserId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Uploads exactly ten DOCX papers for a new book.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(110 * 1024 * 1024)]
    public async Task<ActionResult<BookUploadResult>> Upload(
        [FromForm] string bookName,
        [FromForm] List<IFormFile> papers,
        CancellationToken cancellationToken)
    {
        var ownerUserId = GetOwnerUserId();

        var uploadedPapers = papers
            .Select(paper => new UploadedPaper(paper.OpenReadStream(), paper.FileName))
            .ToArray();

        try
        {
            var result = await bookUploadService.UploadAsync(
                bookName,
                ownerUserId,
                uploadedPapers,
                cancellationToken);

            return Created($"/api/books/{result.BookId}", result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        finally
        {
            foreach (var paper in uploadedPapers)
            {
                paper.Content.Dispose();
            }
        }
    }

    /// <summary>Starts generating the e-book PDF from uploaded papers.</summary>
    [HttpPost("{id:guid}/create")]
    public async Task<ActionResult<BookStatusResult>> Create(
        Guid id,
        CancellationToken cancellationToken)
    {
        var ownerUserId = GetOwnerUserId();

        try
        {
            var result = await bookGenerationService.StartGenerationAsync(id, ownerUserId, cancellationToken);
            if (result is null)
            {
                return NotFound(new { message = $"Book with ID '{id}' was not found." });
            }

            return Accepted($"/api/books/{id}", result);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    /// <summary>Streams or downloads the generated e-book PDF.</summary>
    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(
        Guid id,
        [FromQuery] bool download,
        CancellationToken cancellationToken)
    {
        var ownerUserId = GetOwnerUserId();

        var book = await bookRepository.GetByIdAsync(id, ownerUserId, cancellationToken);
        if (book is null)
        {
            return NotFound(new { message = $"Book with ID '{id}' was not found." });
        }

        if (book.Status != BookStatus.Completed || string.IsNullOrWhiteSpace(book.PdfPath))
        {
            return BadRequest(new
            {
                message = book.Status switch
                {
                    BookStatus.Processing => "E-Book is currently being generated. Please wait.",
                    BookStatus.Failed => $"E-Book generation failed: {book.ErrorMessage}",
                    _ => "E-Book generation has not been initiated."
                }
            });
        }

        var pdfPath = book.PdfPath!;
        if (!await fileStorage.ExistsAsync(pdfPath, cancellationToken))
        {
            return NotFound(new { message = "Generated PDF file could not be found." });
        }

        var stream = await fileStorage.OpenReadAsync(pdfPath, cancellationToken);
        var downloadFileName = $"{book.Name}.pdf";

        if (download)
        {
            return File(stream, "application/pdf", downloadFileName, enableRangeProcessing: true);
        }

        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{Uri.EscapeDataString(downloadFileName)}\"");
        return File(stream, "application/pdf", enableRangeProcessing: true);
    }

    private string GetOwnerUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? "development-user";
}
