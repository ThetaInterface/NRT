using static NRT.IO.IO;

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

    public static bool Write(this Config config) =>
        TrySerialize(config, Config.CONFIG_FILE_NAME);

    public static bool Read(this ref Config config) =>
        TryDeserialize(ref config, Config.CONFIG_FILE_NAME);
}