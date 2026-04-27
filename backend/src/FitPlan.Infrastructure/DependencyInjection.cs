using Microsoft.Extensions.DependencyInjection;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Infrastructure.Persistence;
using FitPlan.Infrastructure.Persistence.Repositories;
using FitPlan.Infrastructure.Services;

namespace FitPlan.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMongoContext, MongoContext>();

        services.AddScoped<ITrainerRepository, TrainerRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailTokenRepository, EmailTokenRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentTrainerAccessor, CurrentTrainerAccessor>();

        services.AddHttpClient<IGoogleOAuthService, GoogleOAuthService>();

        services.AddHttpContextAccessor();

        return services;
    }
}
