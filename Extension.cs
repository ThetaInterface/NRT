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
}