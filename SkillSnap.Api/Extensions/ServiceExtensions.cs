using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using System.Text.Json.Serialization;

namespace SkillSnap.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers with JSON configuration
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Handle reference cycles
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                
                // Optional: Preserve references instead of ignoring cycles
                // options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                
                // Configure property naming policy (optional)
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep PascalCase
                
                // Handle null values
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        // Database
        services.AddDatabase(configuration);

        // API Documentation (Swagger)
        services.AddSwaggerDocumentation();

        // CORS
        services.AddCorsPolicy();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SkillSnapContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Database connection string not found.")));

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "SkillSnap API",
                Version = "v1",
                Description = "API for managing portfolio users, projects, and skills",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "SkillSnap Development Team",
                    Email = "dev@skillsnap.com"
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowClient", policy =>
            {
                policy.WithOrigins(
                        // "https://localhost:5173", 
                        // "http://localhost:5173",
                        // "https://localhost:7128", 
                        "http://localhost:5179"
                        // "https://localhost:5001",
                        // "http://localhost:5000",
                        // "http://localhost:5217"
                        // "https://localhost:5217"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SkillSnap API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "SkillSnap API Documentation";
            });
        }

        return app;
    }
}
