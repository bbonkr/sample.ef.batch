
### Generate migration code

```powershell
PS> cd src/Sample.Data
PS> dotnet ef migrations add "Initialize database" --context AppDbContext --startup-project ../Sample.App --project ../Sample.Data.SqlServer --json
```
