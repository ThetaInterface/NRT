namespace NRT.Core;

public static class Extension
{
    public static string Display(this ELogLevel level) => level.ToString().ToUpper();
}