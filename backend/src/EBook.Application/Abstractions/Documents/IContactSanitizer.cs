using EBook.Application.Documents;

namespace EBook.Application.Abstractions.Documents;

public interface IContactSanitizer
{
    DocumentContent Sanitize(DocumentContent content);
}