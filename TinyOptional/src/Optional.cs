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

    public static Optional<T> Of(T? value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return new Optional<T>(value, true);
    }

    public static Optional<T> OfNullable(T? value)
    {
        return new Optional<T>(value, value != null);
    }

    public static Optional<T> Empty()
    {
        return new Optional<T>(default, false);
    }

    public static Optional<TU> Empty<TU>()
    {
        return new Optional<TU>(default, false);
    }

    public bool IsPresent()
    {
        return _hasValue;
    }

    public T Get()
    {
        if (!_hasValue) throw new InvalidOperationException("No value present");
        return _value!;
    }

    public T GetOrThrow(Func<Exception> exceptionSupplier)
    {
        if (_hasValue) return _value!;
        throw exceptionSupplier();
    }

    public T OrElse(T other)
    {
        return _hasValue ? _value! : other;
    }

    public T OrElseGet(Func<T> other)
    {
        return _hasValue ? _value! : other();
    }

    public T OrElseDo(Action other)
    {
        if (!_hasValue) other.Invoke();
        return _value!;
    }

    public T OrElseThrow(Func<Exception> exceptionSupplier)
    {
        if (_hasValue) return _value!;
        throw exceptionSupplier();
    }

    public Optional<T> Where(Func<T, bool> predicate)
    {
        if (!_hasValue) return this;
        return predicate(_value!) ? this : Empty();
    }

    public Optional<TResult> Select<TResult>(Func<T, TResult> mapper)
    {
        return !_hasValue ? Empty<TResult>() : Optional<TResult>.Of(mapper(_value!));
    }

    public Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> mapper)
    {
        return !_hasValue ? Empty<TResult>() : mapper(_value!);
    }

    public OptionalIfPresentResult<T> IfPresent(Action<T> action)
    {
        if (_hasValue) action(_value!);
        return new OptionalIfPresentResult<T>(this);
    }

    public OptionalIfPresentResult<T> Then(Action<T> action)
    {
        if (_hasValue) action(_value!);
        return new OptionalIfPresentResult<T>(this);
    }

    public async Task IfPresentAsync(Func<T, Task> action)
    {
        if (_hasValue) await action(_value!);
    }

    public OptionalIfNotPresentResult<T> IfNotPresent(Action action)
    {
        if (!_hasValue) action();
        return new OptionalIfNotPresentResult<T>(this);
    }

    public async Task IfNotPresentAsync(Func<Task> asyncAction)
    {
        if (!_hasValue) await asyncAction();
    }

    public override string ToString()
    {
        return _hasValue ? _value?.ToString() ?? "" : "Empty";
    }

    public bool IsNotPresent()
    {
        return !_hasValue;
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
