public class RequestTimeMiddleware
{
    RequestDelegate next;
    ILogger<RequestTimeMiddleware> logger;
    public RequestTimeMiddleware(RequestDelegate _next, ILogger<RequestTimeMiddleware> _logger)
    {
        next = _next;
        logger = _logger;
    }

    public async Task InvokeAsync(HttpContext http)
    {
        logger.LogInformation("Income request come from {Method} {Path}",
        http.Request.Method,
        http.Request.Path
        );
        var startTime = DateTime.Now;
        try
        {
            await next(http);
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Something whent wrrong!");
            http.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await http.Response.WriteAsync("Internal server error");
        }

        var endTime = DateTime.Now;
        logger.LogInformation("The request finished!");
        Console.WriteLine($"Request took {(endTime - startTime).Milliseconds} ms");
    }
}
