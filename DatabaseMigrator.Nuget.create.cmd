mkdir publish

.nuget\Nuget.exe pack Src\DatabaseMigrator.Console\DatabaseMigrator.Console.csproj -IncludeReferencedProjects -o publish
.nuget\Nuget.exe pack Src\DatabaseMigrator.Core\DatabaseMigrator.Core.csproj -o publish
.nuget\Nuget.exe pack Src\DatabaseMigrator.PostgreSql\DatabaseMigrator.PostgreSql.csproj -o publish