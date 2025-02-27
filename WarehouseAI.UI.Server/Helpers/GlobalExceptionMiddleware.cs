using System.Net;
using System.Net.Mime;
using System.Text.Json;
using WarehouseAI.UI.Shared;

namespace WarehouseAI.UI.Server.Helpers;

public class GlobalExceptionMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var apiEx = new ApiEx
        {
            Message = exception.Message,
            StatusCode = GetStatusCode(exception),
        };

        if (Enum.IsDefined(typeof(HttpStatusCode), apiEx.StatusCode))
        {
            apiEx.StatusPhrase = ((HttpStatusCode)apiEx.StatusCode).ToString();
        }

        context.Response.StatusCode = apiEx.StatusCode;

        var response = JsonSerializer.Serialize(apiEx, JsonSerializerOptions);
        await context.Response.WriteAsync(response);
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ApplicationException ex when ex.Message.Contains("Invalid Token") => (int)
                HttpStatusCode.Forbidden,
            ApplicationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            InvalidOperationException => (int)HttpStatusCode.Conflict,
            AccessViolationException => (int)HttpStatusCode.Forbidden,
            HttpRequestException => (int)HttpStatusCode.BadGateway,
            _ => (int)HttpStatusCode.InternalServerError,
        };
}