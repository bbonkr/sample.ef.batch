
using Microsoft.Extensions.Logging;

using Sample.Data;
using Sample.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Sample.App.Jobs;

public abstract class RemoveSampleDataJob : JobBase
{
    public RemoveSampleDataJob(AppDbContext context, ILogger<RemoveSampleDataJob> logger) : base(context, logger)
    {
        SetThrowsException();
    }

    public override async Task<long> ExecuteAsync()
    {
        var watch = new Stopwatch();

        var sql = string.Empty;
        sql += $"DELETE FROM {nameof(UserToken)} ";
        sql += $" WHERE {nameof(UserToken.ExpiresAt)} <= GetUtcDate()";

        watch.Start();

        using (var transaction = Context.Database.BeginTransaction())
        {
            try
            {
                await Context.Database.ExecuteSqlRawAsync(sql);

                if (throwsException)
                {
                    throw new Exception("Test");
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
            }
            finally
            {
                watch.Stop();
            }

        }

        return watch.ElapsedMilliseconds;
    }

    public abstract void SetThrowsException();

    protected bool throwsException = false;
}


public class RemoveSampleData1Job : RemoveSampleDataJob
{
    public RemoveSampleData1Job(AppDbContext context, ILogger<RemoveSampleDataJob> logger) : base(context, logger)
    {
    }

    public override void SetThrowsException()
    {
        throwsException = true;
    }
}

public class RemoveSampleData2Job : RemoveSampleDataJob
{
    public RemoveSampleData2Job(AppDbContext context, ILogger<RemoveSampleDataJob> logger) : base(context, logger)
    {
    }

    public override void SetThrowsException()
    {
        throwsException = false;
    }
}
