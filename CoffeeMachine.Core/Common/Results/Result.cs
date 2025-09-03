namespace CoffeeMachine.Core.Common.Results;
public class Result
{
    public bool IsSuccess
    {
        get;
    }
    public string Error
    {
        get;
    }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success()
    {
        return new Result(true, string.Empty);
    }

    public static Result Failure(string error)
    {
        return new Result(false, error);
    }

    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(value, true, string.Empty);
    }

    public static Result<T> Failure<T>(string error)
    {
        return new Result<T>(default, false, error);
    }
}

public class Result<T> : Result
{
    private readonly T _value;

    public T Data => _value;

    protected internal Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        _value = value;
    }
}