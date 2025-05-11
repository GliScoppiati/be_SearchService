using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SearchService.Clients;
using SearchService.Repositories;
using System.Text;
using SearchService.Services; 

var builder = WebApplication.CreateBuilder(args);

// 🔐 JWT settings
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.ASCII.GetBytes(jwtSection["Key"]!);

Console.WriteLine("🔐 JWT Key: " + jwtSection["Key"]);
Console.WriteLine("🔐 JWT Issuer: " + jwtSection["Issuer"]);
Console.WriteLine("🔐 JWT Audience: " + jwtSection["Audience"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
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
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
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

// 🚀 Carica cocktails/ingredienti mappa iniziale
// using (var scope = app.Services.CreateScope())
// {
//     var repo   = scope.ServiceProvider.GetRequiredService<CocktailRepository>();
//     var client = scope.ServiceProvider.GetRequiredService<CocktailServiceClient>();
//     var log    = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

//     for (int attempt = 1; attempt <= 5; attempt++)
//     {
//         await Task.Delay(TimeSpan.FromSeconds(60));
//         await repo.ReloadAsync(client, force: true);

//         log.LogInformation("Tentativo {Attempt}/5 – cache contiene {Count} cocktail.",
//                            attempt, repo.GetCocktails().Count);

//         if (attempt < 5)
//             await Task.Delay(TimeSpan.FromSeconds(5));
//     }

//     log.LogInformation("✅ Bootstrap completato (non garantisce dataset pieno).");
// }


// ✅ HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Console.WriteLine("🕒 UTC NOW from SearchService: " + DateTime.UtcNow);

app.UseCors("AllowAll"); // 👈 IMPORTANTE: Prima di auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("✅ SearchService avviato su: " + builder.Configuration["ASPNETCORE_URLS"]);
app.Run();
