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
    services.AddScoped<RemoveEachSampleDataJob>();

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
        var remove3Job = scope.ServiceProvider.GetRequiredService<RemoveEachSampleDataJob>(); 

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // #1-1 Insert some rows
            var elapsedAddJob = await addJob.ExecuteAsync();
            // not interested
            //logger.LogInformation("Add job #1-1: {elapsed}", elapsedAddJob);

            // #2 Delete rows
            var elapsedRemoveEachJob = await remove3Job.ExecuteAsync();
            logger.LogInformation("Remove each job #2: {elapsed}", elapsedRemoveEachJob);

            // #1-2 Insert some rows
            elapsedAddJob = await addJob.ExecuteAsync();
            // not interested
            //logger.LogInformation("Add job #1-2: {elapsed}", elapsedAddJob);

            // #3 Delete all rows but transaction will be rollbacked. Rows will have to be exists.
            var elapsedRemoveJob1 = await remove1Job.ExecuteAsync();
            // not interested
            logger.LogInformation("Remove job #3: {elapsed}", elapsedRemoveJob1);

            // #4 Delete all rows and commit its transaction. So rows will be gone.
            var elapsedRemoveJob2 = await remove2Job.ExecuteAsync();
            logger.LogInformation("Remove job #4: {elapsed}", elapsedRemoveJob2);           
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
