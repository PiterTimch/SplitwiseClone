using FluentValidation;
using Microsoft.OpenApi;
using SplitwiseClone.Application;
using SplitwiseClone.Api.Exceptions;
using SplitwiseClone.Api.Filters;
using SplitwiseClone.Api.Validations;
using SplitwiseClone.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped<IValidator<SplitwiseClone.Application.DTOs.Auth.RegisterRequestDto>, RegisterRequestDtoValidator>();
builder.Services.AddScoped<IValidator<SplitwiseClone.Application.DTOs.Auth.LoginRequestDto>, LoginRequestDtoValidator>();
builder.Services.AddScoped<IValidator<SplitwiseClone.Application.DTOs.Users.UpdateUserDto>, UpdateUserDtoValidator>();
builder.Services.AddScoped<IValidator<SplitwiseClone.Application.DTOs.Groups.CreateGroupDto>, CreateGroupDtoValidator>();
builder.Services.AddScoped<IValidator<SplitwiseClone.Application.DTOs.Groups.UpdateGroupDto>, UpdateGroupDtoValidator>();
builder.Services.AddScoped<IValidator<SplitwiseClone.Application.DTOs.Groups.AddGroupMemberDto>, AddGroupMemberDtoValidator>();
builder.Services.AddScoped<IValidator<SplitwiseClone.Application.DTOs.Expenses.CreateExpenseDto>, CreateExpenseDtoValidator>();
builder.Services.AddScoped<IValidator<SplitwiseClone.Application.DTOs.Expenses.UpdateExpenseDto>, UpdateExpenseDtoValidator>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();
app.UseExceptionHandler();

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
