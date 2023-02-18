using System.IO;

namespace DatabaseMigrator.Core;

public class SqlMigratorParameters
{
    public SqlMigratorParameters(DirectoryInfo rootDirectory)
    {
        RootDirectory = rootDirectory;
    }

    public DirectoryInfo RootDirectory { get; }
}