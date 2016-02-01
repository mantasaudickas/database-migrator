using System;

namespace DatabaseMigrator.Core
{
    public interface IDatabaseMigratorLogger
    {
        void Info(string message, params object[] args);
    }

    public class ConsoleLogger : IDatabaseMigratorLogger
    {
        public void Info(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }
}
