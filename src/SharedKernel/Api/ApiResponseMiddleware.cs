using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedKernel.Wrappers;

namespace SharedKernel.Api
{
    /// <summary>
    /// Middleware for standardizing API responses
    /// </summary>
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiResponseMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        public ApiResponseMiddleware(RequestDelegate next, ILogger<ApiResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Process the request and standardize the response
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);

                if (ShouldTransformResponse(context))
                {
                    await TransformResponseAsync(context, responseBody, originalBodyStream);
                }
                else
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private bool ShouldTransformResponse(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!context.Response.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? true)
                return false;

            return true;
        }

        private async Task TransformResponseAsync(HttpContext context, MemoryStream responseBody, Stream originalBodyStream)
        {
            int statusCode = context.Response.StatusCode;

            responseBody.Seek(0, SeekOrigin.Begin);

            string responseContent = await new StreamReader(responseBody).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                if (statusCode >= 200 && statusCode < 300)
                {
                    var successResponse = Response.Success();
                    await WriteResponseAsync(context, originalBodyStream, successResponse);
                }
                else
                {
                    var errorResponse = Response.Fail(GetDefaultErrorMessage(statusCode));
                    await WriteResponseAsync(context, originalBodyStream, errorResponse);
                }
                return;
            }

            try
            {
                var responseObject = JsonSerializer.Deserialize<object>(responseContent);

                if (IsResponseWrapped(responseObject))
                {
                    await WriteRawResponseAsync(context, originalBodyStream, responseContent);
                    return;
                }

                if (statusCode >= 200 && statusCode < 300)
                {
                    var successResponse = Response<object>.Success(responseObject);
                    await WriteResponseAsync(context, originalBodyStream, successResponse);
                }
                else
                {
                    string errorMessage = responseObject?.ToString() ?? GetDefaultErrorMessage(statusCode);
                    var errorResponse = Response.Fail(errorMessage);
                    await WriteResponseAsync(context, originalBodyStream, errorResponse);
                }
            }
            catch (JsonException)
            {
                if (statusCode >= 200 && statusCode < 300)
                {
                    var successResponse = Response.Success(responseContent);
                    await WriteResponseAsync(context, originalBodyStream, successResponse);
                }
                else
                {
                    var errorResponse = Response.Fail(responseContent);
                    await WriteResponseAsync(context, originalBodyStream, errorResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming API response");
                var errorResponse = Response.Fail("An error occurred while processing the response");
                await WriteResponseAsync(context, originalBodyStream, errorResponse);
            }
        }

        private async Task WriteResponseAsync<T>(HttpContext context, Stream outputStream, T response)
        {
            var json = JsonSerializer.Serialize(response, _jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength = bytes.Length;
            
            await outputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        private async Task WriteRawResponseAsync(HttpContext context, Stream outputStream, string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength = bytes.Length;
            
            await outputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        private bool IsResponseWrapped(object responseObject)
        {
            if (responseObject is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    return jsonElement.TryGetProperty("succeeded", out _);
                }

             
                return false;
            }

            return false;
        }
        private string GetDefaultErrorMessage(int statusCode)
        {
            return statusCode switch
            {
                400 => "Invalid request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Resource not found",
                500 => "An unexpected error occurred",
                _ => $"HTTP error {statusCode}"
            };
        }
    }

    /// <summary>
    /// Extension methods for the API response middleware
    /// </summary>
    public static class ApiResponseMiddlewareExtensions
    {
        /// <summary>
        /// Use standardized API responses
        /// </summary>
        public static IApplicationBuilder UseStandardizedResponses(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ApiResponseMiddleware>();
        }
    }
}
