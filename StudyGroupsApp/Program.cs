using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StudyGroupsApp.Data;
using StudyGroupsApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Configure Services
// ----------------------------

// Add EF Core with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        // If a real SQL Server is available, use it
        options.UseSqlServer(connectionString);
    }
    else
    {
        // Otherwise, fall back to in-memory database (useful for testing/dev)
        options.UseInMemoryDatabase("TestDb");
    }
});

// Register repositories
builder.Services.AddScoped<IStudyGroupRepository, StudyGroupRepository>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StudyGroups API",
        Version = "v1",
        Description = "API for creating and managing study groups"
    });
});

var app = builder.Build();

// ----------------------------
// Configure HTTP Pipeline
// ----------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyGroups API v1");
        c.RoutePrefix = string.Empty; // Swagger по адресу http://localhost:5000/
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();