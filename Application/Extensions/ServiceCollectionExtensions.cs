using Application.Interfaces;
using Application.Services;
using Application.Validators;
using Core.Interfaces.Authentication;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            // Register FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register application services
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IFrameworkService, FrameworkService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IClauseService, ClauseService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}