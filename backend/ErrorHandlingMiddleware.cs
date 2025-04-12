using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace backend;

[ExcludeFromCodeCoverage]
public class ErrorHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ErrorHandlingMiddleware> _logger;

	public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
			var errorId = Guid.NewGuid();
			_logger.LogError(ex, "Unhandled exception. Error ID: {ErrorId}", errorId);

			context.Response.ContentType = "application/problem+json";
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

			var problem = new ProblemDetails
			{
				Status = StatusCodes.Status500InternalServerError,
				Title = "An unexpected error occurred.",
				Detail = $"Error ID: {errorId}",
			};

			await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
		}
	}
}
