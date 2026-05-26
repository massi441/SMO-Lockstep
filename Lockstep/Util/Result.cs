namespace Lockstep.Util;

internal class Result<T>
{
    private readonly T? _data;

    public bool IsSuccess => _data != null;
    public bool IsFailed => _data == null;

    private Result(T data)
    {
        _data = data;
    }

    private Result()
    {
        _data = default;
    }

    public static Result<T> Success(T data)
    {
        return new Result<T>(data);
    }

    public static Result<T> Failure()
    {
        return new Result<T>();
    }
}

internal class Result<T, E> where E : struct, Enum
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
