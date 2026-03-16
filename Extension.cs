using System.Threading.Tasks;
using NRT.Exception;

namespace NRT.Util;

public static class Extension
{
    public static string Display(this ELogLevel level)
    {
        switch (level)  
        {
            case ELogLevel.Debug:
                return "DEBUG";
            
            case ELogLevel.Warning:
                return "WARNING";

            case ELogLevel.Error:
                return "ERROR";
            
            default:
                return "UNKOWN";
        }
    }

    public static async Task<bool> Write(this Config config) 
    {
        var result = await IO.TrySerializeAsync(config, Config.CONFIG_PATH);

        return result.Success;
    }

    public static async Task<Result<Config>> Read(this Config config) 
    {
        var result = await IO.TryDeserializeAsync<Config>(Config.CONFIG_PATH);

        return result;
    }
}