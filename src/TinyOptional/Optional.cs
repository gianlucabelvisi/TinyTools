namespace TinyOptional;
// ReSharper disable UnusedMember.Global

public class Optional<T> : IEquatable<Optional<T>>
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
    /// <exception cref="ArgumentNullException"></exception>
    public static Optional<T> Of(T? value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return new Optional<T>(value, true);
    }

    /// <summary>
    /// Box a nullable value into an Optional. Returns empty if value is null.
    /// </summary>
    public static Optional<T> OfNullable(T? value)
    {
        return new Optional<T>(value, value != null);
    }

    /// <summary>
    /// Returns an empty Optional.
    /// </summary>
    public static Optional<T> Empty()
    {
        return new Optional<T>(default, false);
    }

    /// <summary>
    /// Returns true if a value is present, false otherwise.
    /// </summary>
    public bool IsPresent() => _hasValue;

    /// <summary>
    /// Returns true if no value is present, false otherwise.
    /// </summary>
    public bool IsNotPresent() => !_hasValue;

    /// <summary>
    /// Returns the value if present, otherwise throws an InvalidOperationException.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public T Get()
    {
        if (!_hasValue) throw new InvalidOperationException("No value present");
        return _value!;
    }

    /// <summary>
    /// Returns the value if present, otherwise throws the exception supplied by <paramref name="exceptionSupplier"/>.
    /// </summary>
    public T GetOrThrow(Func<Exception> exceptionSupplier)
    {
        if (_hasValue) return _value!;
        throw exceptionSupplier();
    }

    /// <summary>
    /// Returns the value if present, otherwise returns <paramref name="other"/>.
    /// </summary>
    public T OrElse(T other)
    {
        return _hasValue ? _value! : other;
    }

    /// <summary>
    /// Returns the value if present, otherwise returns the result of <paramref name="other"/>.
    /// </summary>
    public T OrElseGet(Func<T> other)
    {
        return _hasValue ? _value! : other();
    }

    /// <summary>
    /// Returns the value if present, otherwise returns null.
    /// </summary>
    public T? OrElseNull() => _hasValue ? _value : default;

    /// <summary>
    /// Executes <paramref name="other"/> if no value is present.
    /// </summary>
    public void OrElseDo(Action other)
    {
        if (!_hasValue) other.Invoke();
    }

    /// <summary>
    /// Apply the where clause to potentially empty the Optional.
    /// </summary>
    public Optional<T> Where(Func<T, bool> predicate)
    {
        if (!_hasValue) return this;
        return predicate(_value!) ? this : Empty();
    }

    /// <summary>
    /// Apply an async predicate to potentially empty the Optional.
    /// </summary>
    public async Task<Optional<T>> WhereAsync(Func<T, Task<bool>> predicate)
    {
        if (!_hasValue) return this;
        return await predicate(_value!) ? this : Empty();
    }

    /// <summary>
    /// Transform the value if present and box it, otherwise return an empty Optional.
    /// </summary>
    public Optional<TResult> Select<TResult>(Func<T, TResult> mapper)
    {
        return !_hasValue ? Optional<TResult>.Empty() : Optional<TResult>.Of(mapper(_value!));
    }

    /// <summary>
    /// Asynchronously transform the value if present and box it, otherwise return an empty Optional.
    /// </summary>
    public async Task<Optional<TResult>> SelectAsync<TResult>(Func<T, Task<TResult>> mapper)
    {
        return !_hasValue ? Optional<TResult>.Empty() : Optional<TResult>.Of(await mapper(_value!));
    }

    /// <summary>
    /// Transform the value into another Optional and flatten, avoiding double-wrapping.
    /// Returns empty if this Optional is empty or if the mapper returns empty.
    /// </summary>
    public Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> mapper)
    {
        return !_hasValue ? Optional<TResult>.Empty() : mapper(_value!);
    }

    /// <summary>
    /// Collapse the Optional to a single value by providing handlers for both cases.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onPresent, Func<TResult> onEmpty)
    {
        return _hasValue ? onPresent(_value!) : onEmpty();
    }

    /// <summary>
    /// Returns an IEnumerable with one element if present, or empty if not.
    /// Integrates cleanly with LINQ.
    /// </summary>
    public IEnumerable<T> ToEnumerable()
    {
        if (_hasValue) yield return _value!;
    }

    /// <summary>
    /// Execute the action if a value is present.
    /// </summary>
    public OptionalIfPresentResult<T> IfPresent(Action<T> action)
    {
        if (_hasValue) action(_value!);
        return new OptionalIfPresentResult<T>(this);
    }

    /// <summary>
    /// Execute the async action if a value is present.
    /// </summary>
    public async Task IfPresentAsync(Func<T, Task> action)
    {
        if (_hasValue) await action(_value!);
    }

    /// <summary>
    /// Execute the action if no value is present.
    /// </summary>
    public OptionalIfNotPresentResult<T> IfNotPresent(Action action)
    {
        if (!_hasValue) action();
        return new OptionalIfNotPresentResult<T>(this);
    }

    /// <summary>
    /// Execute the async action if no value is present.
    /// </summary>
    public async Task IfNotPresentAsync(Func<Task> asyncAction)
    {
        if (!_hasValue) await asyncAction();
    }

    public override string ToString()
    {
        return _hasValue ? _value?.ToString() ?? "" : "Empty";
    }

    public bool Equals(Optional<T>? other)
    {
        if (other is null) return false;
        if (!_hasValue && !other._hasValue) return true;
        if (_hasValue != other._hasValue) return false;
        return EqualityComparer<T>.Default.Equals(_value!, other._value!);
    }

    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

    public override int GetHashCode() => _hasValue ? EqualityComparer<T>.Default.GetHashCode(_value!) : 0;

    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);
    public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);
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
