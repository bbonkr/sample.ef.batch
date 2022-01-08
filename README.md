A repository I wrote out of curiosity about the benefits of using raw queries when using EntityFrameworkCore.

## About

### Environment

* Visual Studio 2022 (v17.0.4)
* Localdb (Please see appsettings.json)
* .NET 6
* EntityFrameworkCore v6.0.1

Target is 1,000 rows. 

> if you need dramatic result, Change target rows to up to 100,000. 🤔


### Generate migration code

If entity changes, you should generate migration code.

```powershell
PS> cd src/Sample.Data
PS> dotnet ef migrations add "Initialize database" --context AppDbContext --startup-project ../Sample.App --project ../Sample.Data.SqlServer --json
```

## Jobs

Each job is isolated to measure execution time.

### Add Some rows (#1-1, #1-2)

I think, insert batch feature is good to go.

```sql
-- Declaring variables 

DECLARE @inserted0 TABLE ([Id] bigint, [_Position] [int]);

MERGE [UserToken] USING (
VALUES (@p0, @p1, @p2, @p3, 0),
(@p4, @p5, @p6, @p7, 1),
(@p8, @p9, @p10, @p11, 2),
-- <<SKIP>>
(@p160, @p161, @p162, @p163, 40),
(@p164, @p165, @p166, @p167, 41)) AS i ([CreatedAt], [ExpiresAt], [Purpose], [Token], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([CreatedAt], [ExpiresAt], [Purpose], [Token])
VALUES (i.[CreatedAt], i.[ExpiresAt], i.[Purpose], i.[Token])
OUTPUT INSERTED.[Id], i._Position
INTO @inserted0;
```

### Remove each rows #2

It runs DELETE statement to every row it needs.

This job takes 507 milliseconds.

```csharp
var deleteCandidate = Context.UserTokens.Where(x => x.ExpiresAt <= DateTime.UtcNow);

Context.UserTokens.RemoveRange(deleteCandidate);

await Context.SaveChangesAsync();
```

```sql
-- Declaring variables 

DELETE FROM [UserToken]
WHERE [Id] = @p0;
SELECT @@ROWCOUNT;

DELETE FROM [UserToken]
WHERE [Id] = @p1;
SELECT @@ROWCOUNT;

DELETE FROM [UserToken]
WHERE [Id] = @p2;
SELECT @@ROWCOUNT;

-- <<SKIP>>
```


### Remove rows #3

This job verify raw sql execution and transaction supporting.

It does not delete rows, because Its transaction will be rollback.

### Remove rows #4

Delete rows using raw SQL.

It may depend on DBMS.

I think, to use ANSI SQL for dependency lower when writes sql statement.

This job takes 17 milliseconds.


## References

* [EntityFrameworkCore: Bulk updates](https://docs.microsoft.com/en-us/ef/core/performance/efficient-updating#bulk-updates)