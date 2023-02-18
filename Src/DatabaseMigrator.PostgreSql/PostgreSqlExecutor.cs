﻿using System;
using System.Data;
using DatabaseMigrator.Core;
using Npgsql;
using Npgsql.Logging;

namespace DatabaseMigrator.PostgreSql;

public class PostgreSqlExecutor : ISqlExecutor
{
    private NpgsqlConnection _currentConnection;
    private PostgreSqlTransaction _currentTransaction;

    public PostgreSqlExecutor(string connectionString, bool debug, bool trace)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        if (trace)
        {
            NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Trace, true, true);
            NpgsqlLogManager.IsParameterLoggingEnabled = true;
        }
        else if (debug)
        {
            NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug, true, true);
            NpgsqlLogManager.IsParameterLoggingEnabled = true;
        }

        _currentConnection = new NpgsqlConnection(connectionString);
        _currentConnection.Open();
    }

    public void Dispose()
    {
        if (_currentTransaction != null)
        {
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }

        if (_currentConnection != null)
        {
            if (_currentConnection.State == ConnectionState.Open)
            {
                _currentConnection.Close();
            }

            _currentConnection.Dispose();
            _currentConnection = null;
        }
    }

    public ISqlScriptTransaction StartTransaction()
    {
        if (_currentTransaction == null)
        {
            _currentTransaction = new PostgreSqlTransaction(_currentConnection);
            return _currentTransaction;
        }

        return new PostgreSqlTransactionWrapper(_currentTransaction);
    }

    public bool Initialize()
    {
        CreateSchemaIfNotExists(_currentConnection);
        CreateVersionTableIfNotExists(_currentConnection);

        return true;
    }

    public int GetCurrentVersion(string scopeName)
    {
        var currentVersion = 0;
        using var command = _currentConnection.CreateCommand();
        command.CommandText = "SELECT MAX(Version) FROM \"Database\".\"Versions\" WHERE Scope = @Scope";

        var scopeParameter = command.CreateParameter();
        scopeParameter.ParameterName = "@Scope";
        scopeParameter.Value = scopeName;
        command.Parameters.Add(scopeParameter);

        object value = command.ExecuteScalar();
        if (value is int version)
        {
            currentVersion = version;
        }

        return currentVersion;
    }

    public int Apply(string filePath, string fileContent, string scope, int version)
    {
        var currentVersion = GetCurrentVersion(scope);
        if (currentVersion < version)
        {
            using (var command = _currentConnection.CreateCommand())
            {
                command.CommandText = fileContent;
                command.CommandTimeout = 300;
                command.ExecuteNonQuery();
            }

            IncrementVersion(_currentConnection, filePath, fileContent, scope, version);
        }

        return 0;
    }

    private void CreateSchemaIfNotExists(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE SCHEMA IF NOT EXISTS \"Database\";";
        command.ExecuteNonQuery();
    }

    private void CreateVersionTableIfNotExists(NpgsqlConnection connection)
    {
        bool tableExists;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT EXISTS (SELECT 1 FROM   information_schema.tables WHERE  table_schema = 'Database' AND table_name = 'Versions');";
            tableExists = (bool) command.ExecuteScalar();
        }

        if (!tableExists)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE \"Database\".\"Versions\" ( " +
                                      "Id uuid not null, " +
                                      "Scope varchar(400) not null, " +
                                      "Version int not null, " +
                                      "File varchar(400) not null, " +
                                      "Content text not null, " +
                                      "Created timestamp DEFAULT(current_timestamp) " +
                                      ");";
                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "ALTER TABLE \"Database\".\"Versions\" ADD CONSTRAINT \"PK_DatabaseVersion\" PRIMARY KEY (id);";
                command.ExecuteNonQuery();
            }
        }
    }

    private void IncrementVersion(NpgsqlConnection connection, string filePath, string content, string scope, int version)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO \"Database\".\"Versions\" (Id, Scope, Version, File, Content) VALUES (@Id, @Scope, @Version, @File, @Content)";

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@Id";
            idParameter.Value = Guid.NewGuid();
            command.Parameters.Add(idParameter);

            var scopeParameter = command.CreateParameter();
            scopeParameter.ParameterName = "@Scope";
            scopeParameter.Value = scope;
            command.Parameters.Add(scopeParameter);

            var versionParameter = command.CreateParameter();
            versionParameter.ParameterName = "@Version";
            versionParameter.Value = version;
            command.Parameters.Add(versionParameter);

            var fileParameter = command.CreateParameter();
            fileParameter.ParameterName = "@File";
            fileParameter.Value = filePath;
            command.Parameters.Add(fileParameter);

            var contentParameter = command.CreateParameter();
            contentParameter.ParameterName = "@Content";
            contentParameter.Value = content;
            command.Parameters.Add(contentParameter);

            command.ExecuteNonQuery();
        }
    }
}