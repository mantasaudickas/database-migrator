using System;
using System.Collections.Generic;
using System.IO;

namespace DatabaseMigrator.Core;

public class SqlMigrator
{
    private readonly IDatabaseMigratorLogger _logger;
    private readonly ISqlExecutor _sqlExecutor;
    private readonly string _identity;

    public SqlMigrator(IDatabaseMigratorLogger logger, ISqlExecutor sqlExecutor, string identity)
    {
        _logger = logger;
        _sqlExecutor = sqlExecutor;
        _identity = identity;
    }

    public void Execute(SqlMigratorParameters parameters)
    {
        var root = parameters.RootDirectory;
        if (root == null)
            throw new NullReferenceException("parameters.RootDirectory");

        if (!root.Exists)
            throw new FileNotFoundException("parameters.RootDirectory", root.FullName);

        var initialized = _sqlExecutor.Initialize();
        if (!initialized)
            throw new Exception("Unable to initialize database version");

        // _logger.Info("Searching script folders in: {0}", root.FullName);
        var directories = new List<DirectoryInfo>(root.GetDirectories());
        directories.Sort((d1, d2) => string.Compare(d1.FullName, d2.FullName, StringComparison.OrdinalIgnoreCase));
        // _logger.Info("Found {0} folders.", directories.Count);

        foreach (var directory in directories)
        {
            var files = new List<FileInfo>(directory.GetFiles("*.sql", SearchOption.TopDirectoryOnly));
            files.Sort((f1, f2) => string.Compare(f1.Name, f2.Name, StringComparison.OrdinalIgnoreCase));

            var versions = new HashSet<int>();
            foreach (var file in files)
            {
                var version = GetFileVersion(file);
                if (versions.Contains(version))
                    throw new NotSupportedException("Duplicated version: " + version);
                versions.Add(version);
            }

            var currentVersion = _sqlExecutor.GetCurrentVersion(directory.Name);

            var reported = false;

            for (var i = currentVersion; i < files.Count; ++i)
            {
                if (!reported)
                {
                    _logger.Info($"[{_identity}] Processing folder: {directory.Name}");
                    _logger.Info($"[{_identity}] Current database version: {currentVersion}");
                    reported = true;
                }

                var file = files[i];

                _logger.Info($"[{_identity}] Processing file: {file.Name}");

                var version = GetFileVersion(file);
                _logger.Info($"[{_identity}] Resolved file version: {version}");

                string content = File.ReadAllText(file.FullName);

                using (var transaction = _sqlExecutor.StartTransaction())
                {
                    _sqlExecutor.Apply(file.FullName, content, directory.Name, version);
                    transaction.Commit();
                }

                _logger.Info($"[{_identity}] File applied successfully");
            }

            currentVersion = _sqlExecutor.GetCurrentVersion(directory.Name);

            if (reported)
            {
                _logger.Info($"[{_identity}] Current version: {currentVersion}");
            }
        }
    }

    private int GetFileVersion(FileInfo file)
    {
        string fileName = file.Name;
        int index = fileName.IndexOf(".", StringComparison.Ordinal);
        if (index < 0)
            throw new Exception("Unable to parse file version! File: " + file.FullName);

        string versionPart = fileName.Substring(0, index);

        if (!int.TryParse(versionPart, out var version))
            throw new Exception("Unable to parse file version! File: " + file.FullName);

        if (version == 0)
            throw new Exception("Unable to parse file version! File: " + file.FullName);
        return version;
    }

    public void Test(SqlMigratorParameters parameters)
    {
        using var transaction = _sqlExecutor.StartTransaction();
        Execute(parameters);
    }
}