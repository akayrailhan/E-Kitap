namespace EBook.Application.Books.Upload;

public sealed record UploadedPaper(Stream Content, string OriginalFileName);
