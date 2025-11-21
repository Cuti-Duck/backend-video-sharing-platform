using backend_video_sharing_platform.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred.");

        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            BadRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ForbiddenException => StatusCodes.Status403Forbidden,
            ConflictException => StatusCodes.Status409Conflict,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Title = exception.GetType().Name,
            Detail = exception.Message,
            Status = statusCode,
            Type = $"https://your-api.com/errors/{statusCode}",
            Instance = context.Request.Path
        };

        // Nếu là ValidationException thì trả thêm lỗi validation
        if (exception is ValidationException vex)
        {
            var validationProblem = new HttpValidationProblemDetails(vex.Errors)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://your-api.com/errors/validation"
            };

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
            return true;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}