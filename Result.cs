namespace NRT.Exception;

public record Result<T>(T? Value, bool Success)
{
    public T? Value = Value;
    public bool Success = Success;
}