using System;
using System.Transactions;

namespace DatabaseMigrator.Core
{
    public interface ISqlScriptTransaction : IDisposable
    {
        void Commit();
    }

    public class SqlScriptTransaction : ISqlScriptTransaction
    {
        private TransactionScope _transactionScope;

        public SqlScriptTransaction()
        {
            const TransactionScopeOption requiresNew = TransactionScopeOption.Required;
            const TransactionScopeAsyncFlowOption asyncFlowOption = TransactionScopeAsyncFlowOption.Suppress;

            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = TimeSpan.FromMinutes(10)
            };

            _transactionScope = new TransactionScope(requiresNew, transactionOptions, asyncFlowOption);
        }

        public void Dispose()
        {
            if (_transactionScope != null)
            {
                _transactionScope.Dispose();
                _transactionScope = null;
            }
        }

        public void Commit()
        {
            if (_transactionScope != null)
            {
                _transactionScope.Complete();
            }
        }
    }
}
