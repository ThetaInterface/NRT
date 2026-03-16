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

        if (!CheckFiles())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            
            Console.WriteLine($"File check failed! Check \'{Loger.GetWorkingSpace() ?? "Log file was not initialize!"}\'\nPress any key to close application...");
            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.White;

            return;
        }

        
    }

    private static bool CheckFiles()
    {
        try {
            IO.CreatePath(ENTRIES_PATH);
        } catch (System.Exception ex) {
            Loger.Message(ELogLevel.Error, ex.Message, prefix: "Check");

            return false;
        } 

        return true;
    }

    private static void OnExeptionThrown(object sender, UnhandledExceptionEventArgs e) =>
        Loger.Message(ELogLevel.Error, e.ExceptionObject.ToString() ?? "UnhandledException");

    private static void OnProcessExit(object? sender, EventArgs e) =>
        Console.Clear();
}