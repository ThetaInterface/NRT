using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using NRT.Core;

namespace NRT;

public static class IO
{
    private static readonly JsonSerializerOptions DEFAULT_SERIALIZE_OPTIONS = new() { WriteIndented = true };

    public static async Task<Result<T>> TryDeserializeAsync<T>(string path)
    {
        try {
            using FileStream fs = new(path, FileMode.Open);
            var result = await JsonSerializer.DeserializeAsync<T>(fs, options: DEFAULT_SERIALIZE_OPTIONS) ?? throw new InvalidOperationException();

            return Result<T>.Ok(result);
        } catch (Exception e) {
            Logger.Error(e, prefix: "Deserialize");

            return Result<T>.Fail(e);
        }
    }

    public static async Task<Result<bool>> TrySerializeAsync<T>(T obj, string path)
    {
        try {
            using FileStream fs = new(path, FileMode.Create);
            await JsonSerializer.SerializeAsync(fs, obj, options: DEFAULT_SERIALIZE_OPTIONS);

            return Result<bool>.Ok(true);
        } catch (Exception e) {
            Logger.Error(e, prefix: "Serialize");

            return Result<bool>.Fail(e);
        }
    }
}