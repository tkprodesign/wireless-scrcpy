namespace WirelessScrcpy.Core.Common;

public readonly record struct Result<T>
{
    private readonly T? _value;
    public bool IsSuccess { get; }
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access the value of a failed result.");
    public AppError? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        Error = null;
    }

    private Result(AppError error)
    {
        IsSuccess = false;
        _value = default;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(AppError error) => new(error);
}
