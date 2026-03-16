using System;
using System.IO;

namespace NRT.Util;

public struct Config
{
    public const string CONFIG_FILE_NAME = "config.json";
    public static readonly string CONFIG_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIG_FILE_NAME);
}