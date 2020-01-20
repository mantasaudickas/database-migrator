mkdir publish

dotnet pack Src\DatabaseMigrator.Core\DatabaseMigrator.Core.csproj --output publish
dotnet pack Src\DatabaseMigrator.PostgreSql\DatabaseMigrator.PostgreSql.csproj --output publish

dotnet build Src\DatabaseMigrator.Console\DatabaseMigrator.Console.csproj
.nuget\Nuget.exe pack Src\DatabaseMigrator.Console\DatabaseMigrator.Console.csproj -OutputDirectory publish