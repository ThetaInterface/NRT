using System;
using System.IO;
using System.Text.Json;

using NRT.Util;

namespace NRT.IO;

public static class IO
{
    private static readonly JsonSerializerOptions DEFAULT_SERIALIZE_OPTIONS = new() { WriteIndented = true };

    public static bool TryReadFile(string path, out string result)
    {
        if (File.Exists(path))
        {
            try {
                using (StreamReader sR = new(path))
                    result = sR.ReadToEnd();
                
                return true;
            } catch (Exception e) {
                Loger.Message(ELogLevel.Error, e.Message, prefix: "Read");

                result = string.Empty;
                return false;
            }
        }
        else
        {
            Loger.Message(ELogLevel.Warning, $"File does not exists! {path}", prefix: "Read");

            result = string.Empty;
            return false;
        }
    }

    public static bool TryWriteFile(string path, string text, bool append = false)
    {
        if (File.Exists(path))
        {
            try {
                using (StreamWriter sW = new(path, append))
                    sW.Write(text);

                return true;
            } catch (Exception e) {
                Loger.Message(ELogLevel.Error, e.Message, prefix: "Write");

                return false;
            }
        }
        else
        {
            Loger.Message(ELogLevel.Warning, $"File does not exists! {path}", prefix: "Write");

            return false;
        }   
    }

    public static bool TryDeserialize<T>(ref T obj, string path)
    {
        if (TryReadFile(path, out string json))
        {
            try {
                T? temp = JsonSerializer.Deserialize<T>(json);

                if (temp != null)
                    obj = temp;
                else
                    return false;

                return true;
            } catch {
                return false;
            }
        }
        else
            return false;
    }

    public static bool TrySerialize<T>(T obj, string path)
    {
        string json = JsonSerializer.Serialize(obj, DEFAULT_SERIALIZE_OPTIONS);

        if (File.Exists(path))
            return TryWriteFile(path, json);
        else
            return false;    
    }
}