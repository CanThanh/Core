using Authorization.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Authorization;

public static class AuthorizationModule
{
    public static IServiceCollection AddAuthorizationModule(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register permission service
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }
}
