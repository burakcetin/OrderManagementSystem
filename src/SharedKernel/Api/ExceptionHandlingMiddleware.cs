using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentValidation;
using SharedKernel.Wrappers;
using Microsoft.AspNetCore.Builder;

namespace SharedKernel.Api
{
    /// <summary>
    /// Middleware for handling exceptions in a standardized way
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Process the request and handle any exceptions
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = exception switch
            {
                ValidationException validationEx => 
                    CreateErrorResponse(validationEx),
                
                KeyNotFoundException notFoundEx => 
                    CreateNotFoundResponse(notFoundEx),
                
                UnauthorizedAccessException unauthorizedEx => 
                    CreateUnauthorizedResponse(unauthorizedEx),
                
                ArgumentException argEx => 
                    CreateBadRequestResponse(argEx),
                
                _ => CreateServerErrorResponse(exception)
            };

            context.Response.StatusCode = GetStatusCode(exception);
            
            var result = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await context.Response.WriteAsync(result);
        }

        private static Response CreateErrorResponse(ValidationException exception)
        {
            var errors = exception.Errors.Select(e => e.ErrorMessage).ToArray();
            return Response.Fail("Validation failed", errors);
        }

        private static Response CreateNotFoundResponse(KeyNotFoundException exception)
        {
            return Response.Fail(exception.Message);
        }

        private static Response CreateUnauthorizedResponse(UnauthorizedAccessException exception)
        {
            return Response.Fail(exception.Message);
        }

        private static Response CreateBadRequestResponse(ArgumentException exception)
        {
            return Response.Fail(exception.Message);
        }

        private static Response CreateServerErrorResponse(Exception exception)
        {
            return Response.Fail("An unexpected error occurred. Please try again later.");
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// Extension methods for configuring the exception handling middleware
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Uses the global exception handler middleware
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
