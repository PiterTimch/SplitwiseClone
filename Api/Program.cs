using SplitwiseClone.Application;
using SplitwiseClone.Api.Middleware;
using SplitwiseClone.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SplitwiseClone API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
