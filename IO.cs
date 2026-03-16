using System.IO;
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
            Logger.Message(ELogLevel.Error, ex.Message, prefix: "Deserialize");

            return new (default, false);
        }
    }

    public static async Task<Result<T>> TrySerializeAsync<T>(T obj, string path)
    {
        try {
            using FileStream fs = new(path, FileMode.OpenOrCreate);
            
            await JsonSerializer.SerializeAsync(fs, obj, options: DEFAULT_SERIALIZE_OPTIONS);
            return new (default, true);
        } catch (System.Exception ex) {
            Logger.Message(ELogLevel.Error, ex.Message, prefix: "Serialize");

            return new (default, false);
        }
    }

    public static async Task<bool> WriteConfig(Config config) 
    {
        var result = await IO.TrySerializeAsync(config, Config.CONFIG_PATH);

        return result.Success;
    }

    public static async Task<Result<Config>> ReadConfig() 
    {
        var result = await IO.TryDeserializeAsync<Config>(Config.CONFIG_PATH);

        return result;
    }
}