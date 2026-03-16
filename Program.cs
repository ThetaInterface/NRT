using System;
using System.IO;
using NRT.Util;

namespace NRT;

public static class Program
{
    private const string DATA_FOLDER_NAME = "data";
    private const string ENTRIES_FOLDER_NAME = "entries";

    private static readonly string APP_PATH = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string DATA_PATH = Path.Combine(APP_PATH, DATA_FOLDER_NAME);
    private static readonly string ENTRIES_PATH = Path.Combine(DATA_PATH, ENTRIES_FOLDER_NAME);

    private static readonly string CONFIG_FILE_PATH = Path.Combine(DATA_PATH, Config.CONFIG_FILE_NAME);

    public static void Main()
    {
        if (!Loger.SetWorkingSpace(DATA_PATH) || !Loger.Message(ELogLevel.Debug, "Loger initialized successfuly!"))
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("Loger was not initilized!\nPress any key to close application...");
            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.White;

            return;
        }

        AppDomain.CurrentDomain.UnhandledException += OnExeptionThrown;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        if (!CheckConfig())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            
            Console.WriteLine("Configuration file was unable to create!\nPress any key to close application...");
            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.White;

            return;
        }
    
        
    }

    private static bool CheckConfig()
    {
        if (!File.Exists(CONFIG_FILE_PATH))
        {
            try {
                File.Create(CONFIG_FILE_PATH).Close();

                return true;
            } catch (Exception e) {
                Loger.Message(ELogLevel.Error, e.Message, prefix: "Config");
                
                return false;
            }
        }
        else
            return true;
    }

    private static void OnExeptionThrown(object sender, UnhandledExceptionEventArgs e) =>
        Loger.Message(ELogLevel.Error, e.ExceptionObject.ToString() ?? "UnhandledException");

    private static void OnProcessExit(object? sender, EventArgs e) =>
        Console.Clear();
}