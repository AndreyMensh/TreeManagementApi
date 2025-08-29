using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TreeManagementApi.Api.Middleware;
using TreeManagementApi.Application.Interfaces;
using TreeManagementApi.Application.Services;
using TreeManagementApi.Infrastructure.Data;
using TreeManagementApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<TreeManagementDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITreeNodeRepository, TreeNodeRepository>();
builder.Services.AddScoped<IExceptionJournalRepository, ExceptionJournalRepository>();
builder.Services.AddScoped<ITreeService, TreeService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Tree Management API", 
        Version = "v1",
        Description = "RESTful API for managing hierarchical tree structures with comprehensive exception logging"
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tree Management API v1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
    
    // Auto-apply migrations in development
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TreeManagementDbContext>();
    try
    {
        context.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error applying database migrations");
    }
}

// Add global exception handling middleware FIRST
app.UseGlobalExceptionHandling();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Redirect root to Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.Logger.LogInformation("Tree Management API starting up...");
app.Run();
