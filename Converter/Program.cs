using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

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
                HideConsoleWindow();
                Application.Run(new MainForm());
            }
            else
            {
                ExecuteCommand();

                while (Console.ReadLine() != "exit")
                    Thread.Sleep(100);
            }
        }

        private static void HideConsoleWindow()
        {
            Console.Title = "Moe2857~" + DateTime.Now.Ticks;
            IntPtr intptr = FindWindow("ConsoleWindowClass", Console.Title);
            ShowWindow(intptr, 0);
        }

        private static void ExecuteCommand()
        {
            if (!CommandLine.TryGetOptionValue("sqlConnString", out string sqlConnString))
                throw new Exception("option \"-sqlConnString\" is require.");
            if (!CommandLine.TryGetOptionValue("sqlitePath", out string sqlitePath))
                throw new Exception("option \"-sqlitePath\" is require.");

            var selected_tables = CommandLine.ValueOptions.Where(x => x.Name == "table").Select(x => x.Value.Trim()).ToArray();

            CommandLine.TryGetOptionValue("password", out string password);

            var allTable = CommandLine.ContainSwitchOption("allTable");

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
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine($"{percent} : {msg}");
                }
            }, schema => schema.Where(x => selected_tables.Any(y => allTable || y.Equals(x.TableName, StringComparison.InvariantCultureIgnoreCase))).ToList()
            , null, createTriggers, createViews, onlyMigrateTableStruct
            );
        }
    }
}