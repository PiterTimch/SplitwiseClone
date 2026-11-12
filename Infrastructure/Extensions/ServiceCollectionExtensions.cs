namespace SplitwiseClone.Infrastructure.Extensions;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Infrastructure.Data;
using SplitwiseClone.Infrastructure.Repositories.Implementations;
using SplitwiseClone.Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString) &&
            Uri.TryCreate(connectionString, UriKind.Absolute, out var databaseUri) &&
            (databaseUri.Scheme == "postgres" || databaseUri.Scheme == "postgresql"))
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.IsDefaultPort ? 5432 : databaseUri.Port,
                Database = databaseUri.AbsolutePath.Trim('/'),
                Username = Uri.UnescapeDataString(databaseUri.UserInfo.Split(':', 2)[0]),
                Password = databaseUri.UserInfo.Contains(':')
                    ? Uri.UnescapeDataString(databaseUri.UserInfo.Split(':', 2)[1])
                    : string.Empty,
                SslMode = Npgsql.SslMode.Require
            };

            connectionString = builder.ConnectionString;
        }

        services.AddDbContext<SplitwiseDbContext>(options =>
            options.UseNpgsql(connectionString));

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<SplitwiseDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Infrastructure implementations for Application contracts
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IExpenseSplitRepository, ExpenseSplitRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        return services;
    }
}
