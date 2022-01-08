using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Sample.Data;
using Sample.Entities;

namespace Sample.App.Jobs;

public class AddSampleDataJob : JobBase
{
    public AddSampleDataJob(AppDbContext context, ILogger<AddSampleDataJob> logger) : base(context, logger)
    {
    }

    public override async Task<long> ExecuteAsync()
    {
        var watch = new Stopwatch();
        watch.Start();
        for (var i = 0; i < 1000; i++)
        {
            Context.UserTokens.Add(CreateUserToken());
        }

        await Context.SaveChangesAsync();

        watch.Stop();

        return watch.ElapsedMilliseconds;
    }

    private UserToken CreateUserToken()
    {
        return new UserToken
        {
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow,
            Token = "Hello token",
            Purpose = "Test",
        };
    }
}
