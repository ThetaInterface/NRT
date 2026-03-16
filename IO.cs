using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using NRT.Exception;
using NRT.Util;

namespace NRT;

public static class IO
{
    private static readonly JsonSerializerOptions DEFAULT_SERIALIZE_OPTIONS = new() { WriteIndented = true };

    public static async Task<Result<T>> TryDeserializeAsync<T>(string path)
    {
        try {
            using FileStream fs = new(path, FileMode.Open);

            return new (await JsonSerializer.DeserializeAsync<T>(fs, options: DEFAULT_SERIALIZE_OPTIONS), true);
        } catch (System.Exception ex) {
            Loger.Message(ELogLevel.Error, ex.Message, prefix: "Deserialize");

            return new (default, false);
        }
    }

    public static async Task<Result<T>> TrySerializeAsync<T>(T obj, string path, bool openOrCreate = false)
    {
        try {
            using FileStream fs = new(path, FileMode.OpenOrCreate);
            
            await JsonSerializer.SerializeAsync(fs, obj, options: DEFAULT_SERIALIZE_OPTIONS);
            return new (default, true);
        } catch (System.Exception ex) {
            Loger.Message(ELogLevel.Error, ex.Message, prefix: "Serialize");

            return new (default, false);
        }
    }

    public static void CreatePath(string path) 
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
}