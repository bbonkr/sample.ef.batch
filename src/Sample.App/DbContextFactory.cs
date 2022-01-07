using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sample.Data;

namespace Sample.App;

public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public const string CONNECTION_STRING_DEFAULT = "Default";

    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new ConfigurationBuilder();
        var configuration = builder.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", true)
              .AddEnvironmentVariables()
              .AddCommandLine(args)
              .Build();

        var connectionString = configuration.GetConnectionString(CONNECTION_STRING_DEFAULT);

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        dbContextOptionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
        {
            sqlServerOptions.MigrationsAssembly(typeof(Sample.Data.SqlServer.PlaceHolder).Assembly.FullName);
        });

        var dbContextOptions = dbContextOptionsBuilder.Options;

        return new AppDbContext(dbContextOptions);
    }
}
