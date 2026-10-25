using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace PharmaFind.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Erreur non gérée lors de la requête {Method} {Path}", context.Request.Method, context.Request.Path);

        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException unauthorized => (HttpStatusCode.Forbidden, unauthorized.Message),
            KeyNotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
            ArgumentException arg => (HttpStatusCode.BadRequest, arg.Message),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "Conflit de synchronisation : cette ressource a été modifiée par une autre opération. Veuillez recharger."),
            DbUpdateException => (HttpStatusCode.BadRequest, "Erreur d'intégrité des données dans la base."),
            _ => (HttpStatusCode.InternalServerError, "Une erreur inattendue est survenue sur le serveur.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
