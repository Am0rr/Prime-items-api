using Microsoft.EntityFrameworkCore;
using PI.DAL.Persistence;
using PI.DAL;
using DotNetEnv;
using System.Text.Json.Serialization;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var connectionString = $"Server={Env.GetString("DB_HOST", "localhost")},{Env.GetString("DB_PORT", "1433")};" +
                       $"Database={Env.GetString("DB_NAME", "PIDb")};" +
                       $"User Id=sa;" +
                       $"Password={Env.GetString("MSSQL_SA_PASSWORD")};" +
                       $"TrustServerCertificate=True;";

builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDataAccess(builder.Configuration);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Starting database migration...");
        // await context.Database.MigrateAsync();
        logger.LogInformation("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "An error occurred while migrating the database.");
        throw;
    }
}

app.UseAuthorization();
app.MapControllers();

app.Run();