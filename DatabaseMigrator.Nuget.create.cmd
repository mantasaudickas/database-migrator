mkdir publish

.nuget\Nuget.exe pack Src\DatabaseMigrator.Console\DatabaseMigrator.Console.csproj -IncludeReferencedProjects -OutputDirectory publish
.nuget\Nuget.exe pack Src\DatabaseMigrator.Core\DatabaseMigrator.Core.csproj -OutputDirectory publish
.nuget\Nuget.exe pack Src\DatabaseMigrator.PostgreSql\DatabaseMigrator.PostgreSql.csproj -OutputDirectory publish