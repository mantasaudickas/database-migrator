using System;

namespace DatabaseMigrator.Core;

public interface ISqlExecutor : IDisposable
{
    ISqlScriptTransaction StartTransaction();
    bool Initialize();
    int GetCurrentVersion(string scopeName);
    int Apply(string filePath, string fileContent, string scope, int version);
}