using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SearchService.Clients;
using SearchService.Repositories;
using SearchService.Services;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Recupera il logger di Program
var logger = builder.Logging.Services.BuildServiceProvider()
                  .GetRequiredService<ILogger<Program>>();

// 🔐 JWT settings
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes   = Encoding.ASCII.GetBytes(jwtSection["Key"]!);

logger.LogDebug("[SearchService] 🔐 JWT Config - Key length: {KeyLength}, Issuer: {Issuer}, Audience: {Audience}",
    jwtSection["Key"]?.Length, jwtSection["Issuer"], jwtSection["Audience"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.SaveToken            = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSection["Issuer"],
            ValidAudience            = jwtSection["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ServiceOnly", policy =>
        policy.RequireRole("Service"));
    options.AddPolicy("AdminOrService", policy =>
        policy.RequireRole("Admin", "Service"));
});

// 🌍 HTTP Client -> CocktailService
builder.Services.AddTransient<JwtServiceHandler>();
builder.Services.AddTransient<ClearAuthHeaderHandler>();
builder.Services.AddHttpClient<CocktailServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://cocktail-service");
})
.AddHttpMessageHandler<ClearAuthHeaderHandler>()
.AddHttpMessageHandler<JwtServiceHandler>();

// 📦 Repository e Storages
builder.Services.AddSingleton<CocktailRepository>();

// ✅ Controller + Swagger + JWT Bearer support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Search API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ✅ CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHostedService<RefreshJob>();

var app = builder.Build();

// 🟢 Bootstrap log (UTC now)
logger.LogInformation("[SearchService] 🕒 UTC NOW: {UtcNow}", DateTime.UtcNow);

// ✅ HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // 👈 IMPORTANTE: prima di Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 🟢 Avvio del service
var urls = builder.Configuration["ASPNETCORE_URLS"] ?? "non configurato";
logger.LogInformation("[SearchService] ✅ Service avviato su: {Urls}", urls);

app.Run();
