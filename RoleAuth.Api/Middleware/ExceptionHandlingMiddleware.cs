using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RoleAuth.Api.Exceptions;

namespace RoleAuth.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Необроблена помилка");
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "Внутрішня помилка сервера");
        }
    }

    private static Task WriteAsync(HttpContext context, int status, string detail)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = ReasonPhrases.GetReasonPhrase(status),
            Detail = detail
        });
    }
}
