using System;
using System.IO;
using System.Threading.Tasks;

using NRT.Resource;
using NRT.Util;
using NRT.Exception;

namespace NRT;

public static class Program
{
    private const string DATA_FOLDER_NAME = "data";
    private const string ENTRIES_FOLDER_NAME = "entries";

    private static readonly string APP_PATH = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string DATA_PATH = Path.Combine(APP_PATH, DATA_FOLDER_NAME);
    private static readonly string ENTRIES_PATH = Path.Combine(DATA_PATH, ENTRIES_FOLDER_NAME);

    public static async Task Main()
    {
        if (!Logger.SetWorkingSpace(DATA_PATH) || !Logger.Message(ELogLevel.Debug, "Loger initialized successfully!"))
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("Logger was not initialized!\nPress any key to close application...");
            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.White;

            return;
        }

        AppDomain.CurrentDomain.UnhandledException += OnExceptionThrown;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        if (!CheckFiles())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            
            Console.WriteLine($"File check failed! Check \'{Logger.GetWorkingSpace() ?? "Log file was not initialize!"}\'\nPress any key to close application...");
            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.White;

            return;
        }

        
    }

    private static bool CheckFiles()
    {
        try {
            Directory.CreateDirectory(ENTRIES_PATH);
        } catch (System.Exception ex) {
            Logger.Message(ELogLevel.Error, ex.Message, prefix: "Check");

            return false;
        } 

        return true;
    }

    private static void OnExceptionThrown(object sender, UnhandledExceptionEventArgs e) =>
        Logger.Message(ELogLevel.Error, e.ExceptionObject.ToString() ?? "UnhandledException");

    private static void OnProcessExit(object? sender, EventArgs e) =>
        Console.Clear();
}