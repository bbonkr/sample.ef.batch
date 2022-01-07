using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Sample.App;
using Sample.App.Jobs;
using Sample.Data;


static IServiceCollection ConfigureService(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString(DbContextFactory.CONNECTION_STRING_DEFAULT);

    services.AddLogging(configure => configure.AddConsole());

    services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlServerOptions =>
        {
            sqlServerOptions.MigrationsAssembly(typeof(Sample.Data.SqlServer.PlaceHolder).Assembly.FullName);
        });
    });

    services.AddScoped<AddSampleDataJob>();
    
    services.AddScoped<RemoveSampleData1Job>();
    services.AddScoped<RemoveSampleData2Job>();

    return services;
}

static async Task PrepareDatabaseAsync(IHost app)
{
    using var scope = app.Services.CreateScope();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var logger = loggerFactory.CreateLogger(nameof(PrepareDatabaseAsync));
    var connectionString = configuration.GetConnectionString(DbContextFactory.CONNECTION_STRING_DEFAULT);

    logger.LogInformation("ConnectionStrings[Default]: {connectionString}", connectionString);

    await dbContext.Database.MigrateAsync();

    logger.LogInformation("Database migrations completed");
}

static async Task DoJobAsync(IHost app)
{
    using (var scope = app.Services.CreateScope())
    {
        var addJob = scope.ServiceProvider.GetRequiredService<AddSampleDataJob>();
        var remove1Job = scope.ServiceProvider.GetRequiredService<RemoveSampleData1Job>();
        var remove2Job = scope.ServiceProvider.GetRequiredService<RemoveSampleData2Job>();

        try
        {
            // Insert some rows
            await addJob.ExecuteAsync();
            // Delete all rows but transaction will be rollbacked. Rows will have to be exists.
            await remove1Job.ExecuteAsync();
            // Delete all rows and commit its transaction. So rows will be gone.
            await remove2Job.ExecuteAsync();
        }
        finally
        {

        }
    }
}

using var host = Host.CreateDefaultBuilder(args)
             .ConfigureAppConfiguration(builder =>
             {
                 builder.SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", true)
                 .AddEnvironmentVariables()
                 .AddCommandLine(args);
             })
             .ConfigureServices((builder, services) =>
             {
                 ConfigureService(services, builder.Configuration);
             })
             .Build();

await PrepareDatabaseAsync(host);

await DoJobAsync(host);

await host.RunAsync();
