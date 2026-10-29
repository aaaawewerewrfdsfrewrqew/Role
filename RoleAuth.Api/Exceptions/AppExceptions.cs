namespace RoleAuth.Api.Exceptions;

public abstract class AppException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class BadRequestException(string message) : AppException(StatusCodes.Status400BadRequest, message);
public class UnauthorizedAppException(string message) : AppException(StatusCodes.Status401Unauthorized, message);
public class NotFoundException(string message) : AppException(StatusCodes.Status404NotFound, message);
public class ConflictException(string message) : AppException(StatusCodes.Status409Conflict, message);
public class EmailSendException()
    : AppException(StatusCodes.Status503ServiceUnavailable, "Не вдалося надіслати лист. Спробуйте пізніше");
