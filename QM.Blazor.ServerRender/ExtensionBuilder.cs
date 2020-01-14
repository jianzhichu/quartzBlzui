using BlazAdmin.ServerRender;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace QM.Blazor.ServerRender
{
    public static class ExtensionBuilder
    {
        public static IServiceCollection AddBlazAdmin(this IServiceCollection services)
        {
            services.AddBlazAdmin<DocsDbContext>();
            return services;
        }
        public static IServiceCollection AddBlazAdminEx<TDbContext>(this IServiceCollection services) where TDbContext : IdentityDbContext
        {
            services.AddBlazAdmin<TDbContext>();
            return services;
        }
        public static IApplicationBuilder UseBlazAdmin(this IApplicationBuilder app)
        {
            app.UseAuthorization();
            app.UseAuthentication();
            return app;
        }
    }
}
