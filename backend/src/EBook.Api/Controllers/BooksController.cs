using System.Security.Claims;
using EBook.Application.Books.Status;
using EBook.Application.Books.Upload;
using Microsoft.AspNetCore.Mvc;

namespace EBook.Api.Controllers;

[ApiController]
[Route("api/books")]
public sealed class BooksController(
    IBookUploadService bookUploadService,
    IBookStatusService bookStatusService) : ControllerBase
{
    /// <summary>Returns the current generation state and ordered papers for a book.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookStatusResult>> GetStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? "development-user";

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
        var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? "development-user";

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
}
