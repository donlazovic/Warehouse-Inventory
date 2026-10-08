using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;
using Warehouse.Api.Authorization;
using Warehouse.Api.DemoData;
using Warehouse.Api.Extensions;
using Warehouse.Api.Realtime;
using Warehouse.BusinessLayer;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.Realtime;
using Warehouse.BusinessLayer.Settings;
using Warehouse.DataLayer;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddDataLayer(builder.Configuration);
builder.Services.AddBusinessLayer(builder.Configuration);

builder.Services.AddControllers(options => options.Filters.Add<RealtimeFlushFilter>());

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UidUserIdProvider>();
builder.Services.AddSingleton<IRealtimePublisher, SignalRPublisher>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            RoleClaimType = AppClaimTypes.Role,
            NameClaimType = AppClaimTypes.FullName,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = token;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

static string ClientIp(HttpContext context)
    => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

static string UserOrIp(HttpContext context)
    => context.User.FindFirst(AppClaimTypes.UserId)?.Value is { } userId
        ? $"user:{userId}"
        : $"ip:{ClientIp(context)}";

static FixedWindowRateLimiterOptions PerMinute(int permits) => new()
{
    PermitLimit = permits,
    Window = TimeSpan.FromMinutes(1),
    QueueLimit = 0
};

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientIp(context), _ => PerMinute(10)));

    options.AddPolicy("refresh", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientIp(context), _ => PerMinute(60)));

    options.AddPolicy("export", context =>
        RateLimitPartition.GetFixedWindowLimiter(UserOrIp(context), _ => PerMinute(10)));

    options.OnRejected = async (context, ct) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            statusCode = StatusCodes.Status429TooManyRequests,
            message = "Previse zahteva u kratkom roku. Sacekajte minut pa pokusajte ponovo."
        }, ct);
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("client", policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Warehouse API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Unesi samo token, bez prefiksa Bearer."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (args.Contains("--reset-data") || args.Contains("--seed-demo"))
{
    await DemoDataCommand.RunAsync(app, seed: args.Contains("--seed-demo"));
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.SeedAdminUserAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("client");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapHub<LiveHub>("/hubs/live");

app.Run();
