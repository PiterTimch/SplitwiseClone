namespace SplitwiseClone.Application;

using Microsoft.Extensions.DependencyInjection;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IExpenseService, ExpenseService>();

        return services;
    }
}
