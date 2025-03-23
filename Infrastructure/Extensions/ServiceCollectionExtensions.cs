using Application.Interfaces;
using Core.Common;
using Core.Interfaces;
using Core.Interfaces.Authentication;
using Core.Interfaces.Repositories;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureLayer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            // Configure SMTP Settings
            services.Configure<SmtpSettings>(configuration.GetSection("SMTP"));

            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor(); // Important for the CurrentTokenProvider
            services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
            services.AddScoped<ICurrentTokenProvider, CurrentTokenProvider>();

            // Register repositories (you can replace this with DI scanning if preferred)
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IClauseRepository, ClauseRepository>();
            services.AddScoped<IClauseCheckListRepository, ClauseCheckListRepository>();
            services.AddScoped<IFrameworkRepository, FrameworkRepository>();
            services.AddScoped<IFrameworkVersionRepository, FrameworkVersionRepository>();
            services.AddScoped<IFrameworkCategoriesRepository, FrameworkCategoriesRepository>();
            services.AddScoped<IFwCatClausesRepository, FwCatClausesRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IOrganizationDepartmentsRepository, OrganizationDepartmentsRepository>();
            services.AddScoped<IOrganizationMembershipRepository, OrganizationMembershipRepository>();
            services.AddScoped<IOrganizationClauseAnswersRepository, OrganizationClauseAnswersRepository>();
            services.AddScoped<IOrganizationCheckListAnswersRepository, OrganizationCheckListAnswersRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionsRepository, RolePermissionsRepository>();

            // Register services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<IEmailService, EmailService>();



            return services;
        }
    }
}