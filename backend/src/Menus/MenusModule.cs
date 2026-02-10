using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Menus;

public static class MenusModule
{
    public static IServiceCollection AddMenusModule(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
