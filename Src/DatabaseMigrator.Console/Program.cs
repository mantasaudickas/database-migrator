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
                bool debug = false;
                bool trace = false;
                bool showHelp = false;
                if (args == null || args.Length < 2)
                {
                    showHelp = true;
                }
                else if (args.Length > 2)
                {
                    for (var i = 2; i < args.Length; i++)
                    {
                        var arg = args[i].Trim();
                        if (string.Compare(arg, "-test", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            test = true;
                        }
                        else if (string.Compare(arg, "-debug", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            debug = true;
                        }
                        else if (string.Compare(arg, "-trace", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            trace = true;
                        }
                        else
                        {
                            showHelp = true;
                        }
                    }
                }

                if (showHelp)
                {
                    ShowHelp();
                    Environment.ExitCode = -1;
                }
                else
                {
                    var parameters = new SqlMigratorParameters(new DirectoryInfo(args[0]));
                    using (var executor = new PostgreSqlExecutor(args[1], debug, trace))
                    {
                        var migrator = new SqlMigrator(new ConsoleLogger(), executor);
                        if (test)
                        {
                            System.Console.WriteLine("Starting database migration test...");
                            migrator.Test(parameters);
                        }
                        else
                        {
                            migrator.Execute(parameters);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                System.Console.WriteLine(exc);
                Environment.ExitCode = -2;
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
            System.Console.WriteLine("-debug - optional argument, enables debug logger");
            System.Console.WriteLine("-trace - optional argument, enables trace logger");
            System.Console.WriteLine("");
            System.Console.WriteLine("NOTE: argument order is important");
        }
    }
}

