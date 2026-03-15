using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ReviewAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Registrera API-controllers
builder.Services.AddControllers();

builder.Services.AddMemoryCache();

// Lägg till DbContext och konfigurera SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hämta JWT-inställningar från appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

// Konfigurera JWT-autentisering
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Konfigurera CORS så att frontenden får anropa API:et
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://minfrontend.se", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

// Aktivera CORS (måste vara före Authentication och Authorization)
app.UseCors("AllowFrontend");

// Aktivera autentisering och auktorisering
app.UseAuthentication();
app.UseAuthorization();

// Aktiverar controllers
app.MapControllers();

// Skapa admin-användare om den inte redan finns
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (!context.Users.Any(u => u.Role == "admin"))
    {
        context.Users.Add(new ReviewAPI.Models.User
        {
            Username = "admin",
            Email = "admin@folio.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "admin"
        });
        context.SaveChanges();
    }
}

app.Run();