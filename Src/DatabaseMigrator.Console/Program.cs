using System;
using System.IO;
using DatabaseMigrator.Core;
using DatabaseMigrator.PostgreSql;

namespace DatabaseMigrator.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //"c:\Projects\DatabaseManager\Sql\PostgreSql" 
                //"Server=localhost;Database=DatabaseManager;User Id=admin;Password=admin"

                bool test = false;
                bool showHelp = false;
                if (args == null || args.Length < 2)
                {
                    showHelp = true;
                }
                else if (args.Length > 2)
                {
                    if (string.Compare(args[2].Trim(), "-test", StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        showHelp = true;
                    }
                    else
                    {
                        test = true;
                    }
                }

                if (showHelp)
                {
                    ShowHelp();
                }
                else
                {
                    var parameters = new SqlMigratorParameters(new DirectoryInfo(args[0]));
                    using (var executor = new PostgreSqlExecutor(args[1]))
                    {
                        var migrator = new SqlMigrator(new ConsoleLogger(), executor);
                        if (test)
                            migrator.Test(parameters);
                        else
                            migrator.Execute(parameters);
                    }
                }
            }
            catch (Exception exc)
            {
                System.Console.WriteLine(exc);
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Console.WriteLine("Press enter to exit");
                System.Console.ReadLine();
            }
        }

        private static void ShowHelp()
        {
            System.Console.WriteLine("Usage: DatabaseMigrator.Console.exe <scriptFolder> <connectionString> [-test]");
            System.Console.WriteLine("");
            System.Console.WriteLine("<scriptFolder> - required argument, specifies root script folder");
            System.Console.WriteLine(
                "<connectionString> - required argument, specifies connection string to server and database");
            System.Console.WriteLine("-test - optional argument, used when testing scripts, transaction rolled back at the end");
            System.Console.WriteLine("");
            System.Console.WriteLine("NOTE: argument order is important");
        }
    }
}

