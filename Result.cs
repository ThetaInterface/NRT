namespace NRT.Exception;

public struct Result<T>(T? value, bool success)
{
    public T? Value = value;
    public bool Success = success;
}