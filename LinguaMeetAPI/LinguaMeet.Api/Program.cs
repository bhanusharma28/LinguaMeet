using System.Text;
using LinguaMeet.Api.Data;
using LinguaMeet.Api.Helpers;
using LinguaMeet.Api.Hubs;
using LinguaMeet.Api.Interfaces.Repositories;
using LinguaMeet.Api.Interfaces.Services;
using LinguaMeet.Api.Middleware;
using LinguaMeet.Api.Repository;
using LinguaMeet.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var b = WebApplication.CreateBuilder(args);
var databaseFile = b.Configuration["DatabaseFile"] ?? "LinguaMeet.db";
var databasePath = Path.Combine(b.Environment.ContentRootPath, databaseFile);
b.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite($"Data Source={databasePath}"));
b.Services.AddScoped<IUserRepository, UserRepository>();
b.Services.AddScoped<IMeetingRepository, MeetingRepository>();
b.Services.AddScoped<ITranscriptRepository, TranscriptRepository>();
b.Services.AddScoped<IAuthService, AuthService>();
b.Services.AddScoped<IUserService, UserService>();
b.Services.AddScoped<IMeetingService, MeetingService>();
b.Services.AddScoped<ITranscriptService, TranscriptService>();
b.Services.AddSingleton<ITranslationService, MockTranslationService>();
b.Services.AddSingleton<JwtHelper>();
var key = Encoding.UTF8.GetBytes(b.Configuration["Jwt:Key"]!);
b.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
        o.Events = new()
        {
            OnMessageReceived = c =>
            {
                var t = c.Request.Query["access_token"];
                if (
                    !string.IsNullOrEmpty(t)
                    && c.HttpContext.Request.Path.StartsWithSegments("/hubs/meeting")
                )
                    c.Token = t;
                return Task.CompletedTask;
            },
        };
    });
b.Services.AddAuthorization();
b.Services.AddControllers();
b.Services.AddSignalR();
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen(o =>
    o.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "Authorization",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "Paste the JWT returned by POST /api/auth/login.",
        }
    )
);
var webOrigin = b.Configuration["WebSettings:Origin"] ?? "https://localhost:7002";
b.Services.AddCors(o =>
    o.AddPolicy(
        "Web",
        p =>
            p.WithOrigins(webOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
    )
);
var app = b.Build();
app.UseMiddleware<ExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "LinguaMeet API v1");
        o.DocumentTitle = "LinguaMeet API";
    });
}
app.UseHttpsRedirection();
app.UseCors("Web");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<MeetingHub>("/hubs/meeting");
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
try
{
    using var scope = app.Services.CreateScope();
    var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await database.Database.MigrateAsync();
    if (app.Environment.IsDevelopment())
        await DbInitializer.SeedDevelopmentUsersAsync(database);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "The database could not be initialized.");
    throw;
}
app.Run();

public partial class Program { }
