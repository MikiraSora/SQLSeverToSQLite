using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DbAccess;
using log4net;
using log4net.Config;

// Configure LOG4NET Using configuration file.
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Converter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            BasicConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!CommandLine.ContainSwitchOption("c"))
            {
                Application.Run(new MainForm());
            }
            else
            {
                ExecuteCommand();
            }
        }

        private static void ExecuteCommand()
        {
            if (!CommandLine.TryGetOptionValue("sqlConnString", out string sqlConnString))
                throw new Exception("option \"-sqlConnString\" is require.");
            if (!CommandLine.TryGetOptionValue("sqlitePath", out string sqlitePath))
                throw new Exception("option \"-sqlitePath\" is require.");

            var selected_tables = CommandLine.ValueOptions.Where(x => x.Name == "table").Select(x => x.Value.Trim()).ToArray();

            CommandLine.TryGetOptionValue("password", out string password);

            if (!CommandLine.TryGetOptionValue("createTriggers", out bool createTriggers))
                createTriggers = false;

            if (!CommandLine.TryGetOptionValue("createViews", out bool createViews))
                createViews = false;

            if (!CommandLine.TryGetOptionValue("onlyMigrateTableStruct", out bool onlyMigrateTableStruct))
                onlyMigrateTableStruct = false;

            SqlServerToSQLite.ConvertSqlServerToSQLiteDatabase(sqlConnString, sqlitePath, password, (done, success, percent, msg) =>
            {
                if (done)
                {
                    Console.WriteLine("Done.");
                }
                else
                {
                    Console.WriteLine($"{percent} : {msg}");
                }
            }, schema => schema.Where(x => selected_tables.Any(y => y.Equals(x.TableName, StringComparison.InvariantCultureIgnoreCase))).ToList()
            , null, createTriggers, createViews, onlyMigrateTableStruct
            );
        }
    }
}