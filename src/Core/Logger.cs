using System;
using System.IO;
using System.Text;

namespace NRT.Core;

public enum ELogLevel : byte
{
    Debug = 0,
    Warning = 1,
    Error = 2
}

public static class Logger
{
    public const string LOG_FILE_NAME = "log.txt";
    public const int LOG_MAX_SIZE_BYTE = 10 * 1024 * 1024; // 10 MB

    public static string? LogFilePath { get; private set; }

    private static FileStream? fs;
    private static StreamWriter? sw;

    private static bool isInitialized = false;

    public static void SetWorkingSpace(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Dispose();

        Directory.CreateDirectory(path);
        LogFilePath = Path.Combine(path, LOG_FILE_NAME);

        fs = new(LogFilePath, FileMode.Append);
        sw = new(fs, Encoding.UTF8);

        isInitialized = true;
    }

    public static void Dispose()
    {
        sw?.Dispose();
        fs?.Dispose();
    }

    public static void Error(Exception? e, string? prefix = null) => 
        Message(ELogLevel.Error, e == null ? "Unhandled Exception" : e.ToString(), prefix);

    public static void Warning(string text, string? prefix = null) => 
        Message(ELogLevel.Warning, text, prefix);

    public static void Debug(string text, string? prefix = null)
    {
        if (ConfigProvider.AppConfig.LogDebugInfo)
            Message(ELogLevel.Debug, text, prefix);
    }

    private static void Message(ELogLevel level, string message, string? prefix = null)
    {
        if (isInitialized)
        {
            string entryContent = $"[{DateTime.Now}] [{level.Display()}] {(prefix != null ? $"[{prefix}] " : string.Empty)}{message}";
            WriteEntry(entryContent);
        }
    }

    private static void WriteEntry(string text)
    {
        if (fs == null || sw == null)
            throw new InvalidOperationException("Logger not initialized! Make sure to call \'SetWorkingSpace()\' first!");

        if (fs.Length >= LOG_MAX_SIZE_BYTE)
        {   
            sw.Flush();
            fs.SetLength(0);
            fs.Seek(0, SeekOrigin.Begin);
        }
        
        sw.WriteLine(text);
        sw.Flush();
    }
}