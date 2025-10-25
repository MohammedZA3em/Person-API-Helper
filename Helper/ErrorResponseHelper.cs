using Microsoft.AspNetCore.Mvc;

public static class ErrorResponseHelper
{
    public static ObjectResult BadRequest(string message)
        => new ObjectResult(new[] { message })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

    public static ObjectResult NotFound(string message)
        => new ObjectResult(new[] { message })
        {
            StatusCode = StatusCodes.Status404NotFound
        };

    public static ObjectResult ServerError(string message = "An unexpected error occurred")
        => new ObjectResult(new[] { message })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
}
