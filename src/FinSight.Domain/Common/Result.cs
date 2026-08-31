namespace FinSight.Domain.Common;

/// <summary>
/// Represents the outcome of an operation, containing execution status and an optional error message.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="error">An optional error message describing why the operation failed.</param>
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error details if the operation failed; otherwise, <see langword="null"/>.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Creates a successful <see cref="Result"/> instance.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() =>
        new(true, null);

    /// <summary>
    /// Creates a failed <see cref="Result"/> instance with a specified error message.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <returns>A failed <see cref="Result"/> containing the error description.</returns>
    public static Result Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new Result(false, error);
    }

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> containing the specified payload value.
    /// </summary>
    /// <typeparam name="T">The payload value type.</typeparam>
    /// <param name="value">The operation result payload.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing <paramref name="value"/>.</returns>
    public static Result<T> Success<T>(T value) =>
        new(value, true, null);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with a specified error message.
    /// </summary>
    /// <typeparam name="T">The payload value type.</typeparam>
    /// <param name="error">The error message describing the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> containing the error description.</returns>
    public static Result<T> Failure<T>(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new(default, false, error);
    }
}

/// <summary>
/// Represents the outcome of an operation that yields a value payload upon success.
/// </summary>
/// <typeparam name="T">The payload value type.</typeparam>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(
        T? value,
        bool isSuccess,
        string? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the payload value of a successful result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing the value on a failed result instance.</exception>
    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "A failed result does not contain a value.");

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a successful <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    public static implicit operator Result<T>(T value) =>
        Success(value);
}
