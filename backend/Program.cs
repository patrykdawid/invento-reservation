using backend;
using backend.Database;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy("defaultPolicy", builder =>
	{
		builder.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader();
	});
});

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IFlightRepository, JsonFlightRepository>();
builder.Services.AddSingleton<IReservationRepository, JsonReservationRepository>();
builder.Services.AddSingleton<JsonDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
	});
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
