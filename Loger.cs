using System;
using System.IO;
using System.Linq;
using System.Text;

namespace NRT.Util;

public static class Loger
{
    private const string LOG_FILE_NAME = "log.txt";

    private static string? logFilePath;

    public static bool SetWorkingSpace(string path)
    {
        try {
            string? directoryPath = Path.GetDirectoryName($"{(!path.EndsWith('\\') ? path + '\\' : path)}");

            if (directoryPath != null)
            {
                CreatePath(directoryPath);

                logFilePath = Path.Combine(directoryPath, LOG_FILE_NAME);

                if (!File.Exists(logFilePath))
                    File.Create(logFilePath).Close();
            }

            return true;
        }
        catch { 
            return false; 
        }
    }

    private static void CreatePath(string path) 
    {
        if (!Directory.Exists(path))
        {
            string childrenName = path.Split('\\', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last();
            string parentPath = path.Replace(childrenName, string.Empty);

            while (parentPath.EndsWith('\\'))
                parentPath = parentPath.TrimEnd('\\');

            if (!Directory.Exists(parentPath))
                CreatePath(parentPath);
            
            Directory.CreateDirectory(path);   
        }
    }

    public static bool Message(ELogLevel level, string message, string? prefix = null)
    {
        string entryContent = $"[{DateTime.Now}] [{level.Display()}] {(prefix != null ? $"[{prefix}]" : string.Empty)} {message}";

        return WriteEntry(entryContent);
    }

    private static bool WriteEntry(string text)
    {
        if (File.Exists(logFilePath))
        {
            try {
                using (StreamWriter sW = new(logFilePath, append: true, Encoding.UTF8))
                    sW.WriteLine(text);     

                return true;
            } catch {
                return false;
            }
        }
        else 
            return false;
    } 
}