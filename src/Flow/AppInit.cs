using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using NRT.Core;

namespace NRT.Flow;

public static partial class App
{
    public static async Task Initialization()
    {   
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        await CheckFiles();

        await ConfigProvider.LoadAsync();

        Logger.SetWorkingSpace(DataPath);
        Logger.Debug("Logger initialized successfully!");

        AppDomain.CurrentDomain.UnhandledException += OnExceptionThrown;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    public static async Task CheckFiles()
    {
        Directory.CreateDirectory(EntriesPath);

        if (!File.Exists(Config.CONFIG_PATH))
            await ConfigProvider.SaveAsync();
    }
        
    private static void OnExceptionThrown(object sender, UnhandledExceptionEventArgs e) 
    {
        Logger.Error(e.ExceptionObject as Exception);

        Write(e.ExceptionObject.ToString());
        Write("\n\n\nPress any key to close application...", nextLine: true);
        ReadKey();

        OnApplicationClosed();
    }

    private static void OnProcessExit(object? sender, EventArgs e) => OnApplicationClosed();

    private static async void OnApplicationClosed()
    {
        await Config.Write(ConfigProvider.AppConfig);

        Logger.Dispose();

        ClearScreen();
    }
}