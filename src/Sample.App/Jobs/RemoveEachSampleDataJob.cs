using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Sample.Data;

namespace Sample.App.Jobs;

public class RemoveEachSampleDataJob : JobBase
{
    public RemoveEachSampleDataJob(AppDbContext context, ILogger<RemoveEachSampleDataJob> logger) : base(context, logger)
    {
    }

    public override async Task<long> ExecuteAsync()
    {
        var watch = new Stopwatch();
        watch.Start();

        var deleteCandidate = Context.UserTokens.Where(x => x.ExpiresAt <= DateTime.UtcNow);

        Context.UserTokens.RemoveRange(deleteCandidate);

        await Context.SaveChangesAsync();

        watch.Stop();

        return watch.ElapsedMilliseconds;
    }
}