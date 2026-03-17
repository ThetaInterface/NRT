using System;
using System.Threading.Tasks;

namespace NRT.Core;

public static class ConfigProvider
{
    private static Config appConfig = new();

    public static Config AppConfig => appConfig;

    public static async Task LoadAsync() => appConfig = await Config.Read();

    public static async Task SaveAsync() => await Config.Write(appConfig);

    public static async Task UpdateAsync(Func<Config, Config> modify)
    {
        appConfig = modify(appConfig);
        await SaveAsync();
    }
}