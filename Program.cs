using Microsoft.EntityFrameworkCore;
using ReviewAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Lägg till DbContext och konfigurera SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();