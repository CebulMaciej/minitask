using System.Net;
using System.Text.Json;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Domain.Exceptions;
using ApplicationException = FitPlan.Application.Common.Exceptions;

namespace FitPlan.Api.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            await HandleAsync(ctx, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var (status, code, message) = ex switch
        {
            NotFoundException e      => (HttpStatusCode.NotFound, "NOT_FOUND", e.Message),
            ConflictException e      => (HttpStatusCode.Conflict, "CONFLICT", e.Message),
            UnauthorizedException e  => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", e.Message),
            ForbiddenException e     => (HttpStatusCode.Forbidden, "FORBIDDEN", e.Message),
            DomainException e        => (HttpStatusCode.BadRequest, "DOMAIN_ERROR", e.Message),
            ValidationException e    => (HttpStatusCode.BadRequest, "VALIDATION_ERROR",
                                          JsonSerializer.Serialize(e.Errors)),
            _                        => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };

        if (status == HttpStatusCode.InternalServerError)
            logger.LogError(ex, "Unhandled exception");

        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { code, message }));
    }
}
