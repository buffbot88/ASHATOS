using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace LegendaryCMS.Core;

/// <summary>
/// Extension methods for registering and configuring LegendaryCMS as a modular, plug-and-play component
/// </summary>
public static class LegendaryCMSExtensions
{
    /// <summary>
    /// Adds LegendaryCMS services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddLegendaryCMS(this IServiceCollection services)
    {
        // Add Razor Pages support for embedded pages
        services.AddRazorPages()
            .AddApplicationPart(Assembly.GetExecutingAssembly())
            .AddRazorRuntimeCompilation(options =>
            {
                // Allow runtime compilation of Razor views from the LegendaryCMS assembly
                options.FileProviders.Add(new EmbeddedFileProvider(
                    Assembly.GetExecutingAssembly(),
                    "LegendaryCMS"
                ));
            });

        // Register CMS services
        services.AddSingleton<ILegendaryCMSModule, LegendaryCMSModule>();
        
        return services;
    }

    /// <summary>
    /// Maps LegendaryCMS routes and endpoints
    /// </summary>
    public static IApplicationBuilder UseLegendaryCMS(this IApplicationBuilder app)
    {
        // LegendaryCMS is now integrated through Razor Pages
        // No additional middleware configuration needed
        return app;
    }
}
