using System;
using System.IO;
using System.Threading.Tasks;
using NRT.Core;

namespace NRT.Flow;

public static partial class App
{
    public const string DATA_FOLDER_NAME = "data";
    public const string ENTRIES_FOLDER_NAME = "entries";

    public static readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string DataPath = Path.Combine(AppPath, DATA_FOLDER_NAME);
    public static readonly string EntriesPath = Path.Combine(DataPath, ENTRIES_FOLDER_NAME);

    public static async Task Main()
    {
        await Initialization();
    }
}