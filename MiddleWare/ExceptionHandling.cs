using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PizzaManagement.MiddleWare
{
    public class ExceptionHandling : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandling> _logger;
        public ExceptionHandling (ILogger<ExceptionHandling> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, 
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred : {message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
