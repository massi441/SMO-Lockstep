namespace SMOO.Util;

internal readonly struct Result<E> where E : struct, Enum
{
    private readonly E? _error;

    public readonly E? Error => _error;

    public bool IsSuccess => _error == null;
    public bool IsFailed => _error != null;

    private Result(E? error)
    {
        _error = error;
    }

    public static Result<E> Success()
    {
        return new Result<E>(null);
    }

    public static Result<E> Failure(E error)
    {
        return new Result<E>(error);
    }
}

internal readonly struct Result<T, E> where E : struct, Enum
{
    private readonly T? _data;
    private readonly E? _error;

    public T? Data => _data;
    public E? Error => _error;

    public bool IsSuccess => _error == null;
    public bool IsFailed => _error != null;

    private Result(T data)
    {
        _data = data;
        _error = null;
    }

    private Result(E error)
    {
        _error = error;
        _data = default;
    }

    public static Result<T, E> Success(T data)
    {
        return new Result<T, E>(data);
    }

    public static Result<T, E> Failure(E error)
    {
        return new Result<T, E>(error);
    }
}
