using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Sample.Data;
using Sample.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sample.App.Jobs;

public class AddSampleDataJob : JobBase
{
    public AddSampleDataJob(AppDbContext context, ILogger<AddSampleDataJob> logger) : base(context, logger)
    {
    }

    public override async Task ExecuteAsync()
    {
        for (var i = 0; i < 1000; i++)
        {
            Context.UserTokens.Add(CreateUserToken());
        }

        await Context.SaveChangesAsync();
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

public abstract class RemoveSampleDataJob : JobBase
{
    public RemoveSampleDataJob(AppDbContext context, ILogger<RemoveSampleDataJob> logger) : base(context, logger)
    {
        SetThrowsException();
    }

    public override async Task ExecuteAsync()
    {
        var sql = string.Empty;
        sql += $"DELETE FROM {nameof(UserToken)} ";
        sql += $" WHERE {nameof(UserToken.ExpiresAt)} <= GetUtcDate()";

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

            }

        }
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