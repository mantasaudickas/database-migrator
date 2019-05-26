using System;
using System.Data;
using DatabaseMigrator.Core;
using Npgsql;

namespace DatabaseMigrator.PostgreSql
{
    public class PostgreSqlTransaction : ISqlScriptTransaction
    {
        private NpgsqlTransaction _transaction;
        private bool _committed;
        private bool _markedForRollback;

        public PostgreSqlTransaction(NpgsqlConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            _transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                if (!_committed)
                {
                    _transaction.Rollback();
                    Console.WriteLine("Dispose: Transaction rolled-back");
                }

                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Commit()
        {
            if (_markedForRollback)
            {
                Console.WriteLine("Commit: Transaction marked for rollback, commit not possible.");
                return;
            }

            if (_transaction != null)
            {
                _transaction.Commit();
                Console.WriteLine("Commit: Transaction committed");
            }
            _committed = true;
        }

        public void MarkForRollback()
        {
            _markedForRollback = true;
        }
    }

    public class PostgreSqlTransactionWrapper : ISqlScriptTransaction
    {
        private readonly PostgreSqlTransaction _parentTransaction;
        private bool _committed;

        public PostgreSqlTransactionWrapper(PostgreSqlTransaction parentTransaction)
        {
            _parentTransaction = parentTransaction;
        }

        public void Dispose()
        {
            if (!_committed)
            {
                _parentTransaction.MarkForRollback();
            }
        }

        public void Commit()
        {
            _committed = true;
        }
    }
}
