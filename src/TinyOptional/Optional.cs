namespace TinyOptional;
// ReSharper disable UnusedMember.Global

public class Optional<T>
{
    private readonly T? _value;
    private readonly bool _hasValue;

    private Optional(T? value, bool hasValue)
    {
        _value = value;
        _hasValue = hasValue;
    }

    /// <summary>
    /// Box a non-null value into an Optional.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Optional<T> Of(T? value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return new Optional<T>(value, true);
    }

    /// <summary>
    /// Box a nullable value into an Optional.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Optional<T> OfNullable(T? value)
    {
        return new Optional<T>(value, value != null);
    }

    /// <summary>
    /// Returns an empty Optional.
    /// </summary>
    /// <returns></returns>
    public static Optional<T> Empty()
    {
        return new Optional<T>(default, false);
    }

    /// <summary>
    /// Returns true if a value is present, false otherwise.
    /// </summary>
    /// <returns></returns>
    public bool IsPresent() => _hasValue;

    /// <summary>
    /// Returns true if a value is not present, false otherwise.
    /// </summary>
    /// <returns></returns>
    public bool IsNotPresent() => !_hasValue;


    /// <summary>
    /// Returns the value if present, otherwise throws an InvalidOperationException.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Get()
    {
        if (!_hasValue) throw new InvalidOperationException("No value present");
        return _value!;
    }

    /// <summary>
    /// Returns the value if present, otherwise throws an exception supplied by <paramref name="exceptionSupplier"/>.
    /// </summary>
    /// <param name="exceptionSupplier"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T GetOrThrow(Func<Exception> exceptionSupplier)
    {
        if (_hasValue) return _value!;
        throw exceptionSupplier();
    }

    /// <summary>
    /// Returns the value if present, otherwise returns <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public T OrElse(T other)
    {
        return _hasValue ? _value! : other;
    }

    /// <summary>
    /// Returns the value if present, otherwise returns the result of <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public T OrElseGet(Func<T> other)
    {
        return _hasValue ? _value! : other();
    }

    /// <summary>
    /// Returns the value if present, otherwise executes <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public T OrElseDo(Action other)
    {
        if (!_hasValue) other.Invoke();
        return _value!;
    }

    /// <summary>
    /// Apply the where clause to potentially empty the Optional.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public Optional<T> Where(Func<T, bool> predicate)
    {
        if (!_hasValue) return this;
        return predicate(_value!) ? this : Empty();
    }

    /// <summary>
    /// Transform the value if present and box it, otherwise return an empty Optional.
    /// </summary>
    /// <param name="mapper"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public Optional<TResult> Select<TResult>(Func<T, TResult> mapper)
    {
        return !_hasValue ? Optional<TResult>.Empty() : Optional<TResult>.Of(mapper(_value!));
    }

    /// <summary>
    /// Execute the action if a value is present.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public OptionalIfPresentResult<T> IfPresent(Action<T> action)
    {
        if (_hasValue) action(_value!);
        return new OptionalIfPresentResult<T>(this);
    }

    /// <summary>
    /// Execute the async action if a value is present.
    /// </summary>
    /// <param name="action"></param>
    public async Task IfPresentAsync(Func<T, Task> action)
    {
        if (_hasValue) await action(_value!);
    }

    /// <summary>
    /// Execute the action if a value is not present.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public OptionalIfNotPresentResult<T> IfNotPresent(Action action)
    {
        if (!_hasValue) action();
        return new OptionalIfNotPresentResult<T>(this);
    }

    /// <summary>
    /// Execute the async action if a value is not present.
    /// </summary>
    /// <param name="asyncAction"></param>
    public async Task IfNotPresentAsync(Func<Task> asyncAction)
    {
        if (!_hasValue) await asyncAction();
    }

    public override string ToString()
    {
        return _hasValue ? _value?.ToString() ?? "" : "Empty";
    }

}

public class OptionalIfPresentResult<T>(Optional<T> optional)
{
    public void OrElse(Action action)
    {
        optional.IfNotPresent(action);
    }
}

public class OptionalIfNotPresentResult<T>(Optional<T> optional)
{
    public void OrElse(Action<T> action)
    {
        optional.IfPresent(action);
    }
}
