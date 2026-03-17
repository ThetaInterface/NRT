using System;
using System.IO;
using System.Threading.Tasks;

using NRT.Flow;

namespace NRT.Core;

public class Config
{
    public const string CONFIG_FILE_NAME = "config.json";
    public static readonly string CONFIG_PATH = Path.Combine(App.DataPath, CONFIG_FILE_NAME);

    public bool LogDebugInfo { get; init; } = false;

    public static async Task<Config> Read()
    {
        if (!File.Exists(CONFIG_PATH))
            throw new InvalidOperationException("Config file does not exist!");

        Result<Config> result = await IO.TryDeserializeAsync<Config>(CONFIG_PATH);

        if (!result.Success)
            throw new InvalidOperationException("Can not read config file!");
        
        return result.Value;
    }

    public static async Task Write(Config config)
    {
        Result<bool> result = await IO.TrySerializeAsync(config, CONFIG_PATH);

        if (!result.Success)
            throw new InvalidOperationException("Can not write config file!");
    }
}