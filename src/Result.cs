using System;
using System.Diagnostics.CodeAnalysis;

namespace NRT;

public readonly record struct Result<T>(T? Value, bool Success, Exception? Exception = null)
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool Success { get; } = Success;

    public static Result<T> Ok(T? value) => new(value, true, null);
    public static Result<T> Fail(Exception exception) => new(default, false, exception);

    public T ValueOrThrow() => Success ? 
        Value : throw Exception ?? throw new InvalidOperationException("Result without error info.");

    public T ValueOr(T defaultValue) => Success ? 
        Value : defaultValue;
}