using FluentAssertions;
using TinyOptional;

namespace TinyOptionalTests;

public class OptionalTests
{
    [Test]
    public void Of_WithNonNullValue_ReturnsOptionalWithValue()
    {
        var optional = Optional<int>.Of(5);
        optional.IsPresent().Should().BeTrue();
        optional.Get().Should().Be(5);
    }

    [Test]
    public void Of_WithNullValue_ThrowsArgumentNullException()
    {
        Action act = () => Optional<string>.Of(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void OfNullable_WithNonNullValue_ReturnsOptionalWithValue()
    {
        var optional = Optional<string>.OfNullable("hello");
        optional.IsPresent().Should().BeTrue();
        optional.Get().Should().Be("hello");
    }

    [Test]
    public void OfNullable_WithNullValue_ReturnsEmptyOptional()
    {
        var optional = Optional<object>.OfNullable(null);
        optional.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void Empty_ReturnsEmptyOptional()
    {
        var optional = Optional<double>.Empty();
        optional.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void IsPresent_WithValue_ReturnsTrue()
    {
        var optional = Optional<bool>.Of(true);
        optional.IsPresent().Should().BeTrue();
    }

    [Test]
    public void IsPresent_WithoutValue_ReturnsFalse()
    {
        var optional = Optional<int>.Empty();
        optional.IsPresent().Should().BeFalse();
    }

    [Test]
    public void IsNotPresent_WithValue_ReturnsFalse()
    {
        var optional = Optional<char>.Of('a');
        optional.IsNotPresent().Should().BeFalse();
    }

    [Test]
    public void IsNotPresent_WithoutValue_ReturnsTrue()
    {
        var optional = Optional<DateTime>.Empty();
        optional.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void Get_WithValue_ReturnsValue()
    {
        var optional = Optional<string>.Of("test");
        optional.Get().Should().Be("test");
    }

    [Test]
    public void Get_WithoutValue_ThrowsInvalidOperationException()
    {
        var optional = Optional<int>.Empty();
        Action act = () => optional.Get();
        act.Should().Throw<InvalidOperationException>().WithMessage("No value present");
    }

    [Test]
    public void GetOrThrow_WithValue_ReturnsValue()
    {
        var optional = Optional<int>.Of(10);
        optional.GetOrThrow(() => new Exception("Custom Exception")).Should().Be(10);
    }

    [Test]
    public void GetOrThrow_WithoutValue_ThrowsSuppliedException()
    {
        var optional = Optional<string>.Empty();
        Action act = () => optional.GetOrThrow(() => new ArgumentException("Custom Exception"));
        act.Should().Throw<ArgumentException>().WithMessage("Custom Exception");
    }

    [Test]
    public void OrElse_WithValue_ReturnsValue()
    {
        var optional = Optional<int>.Of(7);
        optional.OrElse(12).Should().Be(7);
    }

    [Test]
    public void OrElse_WithoutValue_ReturnsOther()
    {
        var optional = Optional<int>.Empty();
        optional.OrElse(15).Should().Be(15);
    }

    [Test]
    public void OrElseGet_WithValue_ReturnsValue()
    {
        var optional = Optional<string>.Of("original");
        optional.OrElseGet(() => "alternative").Should().Be("original");
    }

    [Test]
    public void OrElseGet_WithoutValue_ReturnsResultOfOther()
    {
        var optional = Optional<string>.Empty();
        optional.OrElseGet(() => "alternative").Should().Be("alternative");
    }

    [Test]
    public void OrElseDo_WithValue_ReturnsValueAndDoesNotExecuteAction()
    {
        var optional = Optional<int>.Of(20);
        var actionExecuted = false;
        optional.OrElseDo(() => actionExecuted = true).Should().Be(20);
        actionExecuted.Should().BeFalse();
    }

    [Test]
    public void OrElseDo_WithoutValue_ExecutesActionAndReturnsDefault()
    {
        var optional = Optional<int>.Empty();
        var actionExecuted = false;
        optional.OrElseDo(() => actionExecuted = true);
        actionExecuted.Should().BeTrue();
    }

    [Test]
    public void Where_WithValueAndMatchingPredicate_ReturnsOptionalWithValue()
    {
        var optional = Optional<int>.Of(8);
        optional.Where(x => x > 5).IsPresent().Should().BeTrue();
        optional.Where(x => x > 5).Get().Should().Be(8);
    }

    [Test]
    public void Where_WithValueAndNonMatchingPredicate_ReturnsEmptyOptional()
    {
        var optional = Optional<int>.Of(3);
        optional.Where(x => x > 5).IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void Where_WithoutValue_ReturnsEmptyOptional()
    {
        var optional = Optional<int>.Empty();
        optional.Where(x => x > 5).IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void Select_WithValue_ReturnsOptionalWithTransformedValue()
    {
        var optional = Optional<int>.Of(6);
        optional.Select(x => x * 2).IsPresent().Should().BeTrue();
        optional.Select(x => x * 2).Get().Should().Be(12);
    }

    [Test]
    public void Select_WithoutValue_ReturnsEmptyOptional()
    {
        var optional = Optional<int>.Empty();
        optional.Select(x => x * 2).IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void IfPresent_WithValue_ExecutesAction()
    {
        var optional = Optional<string>.Of("present");
        var actionExecuted = false;
        string? valuePassedToAction = null;
        optional.IfPresent(value =>
        {
            actionExecuted = true;
            valuePassedToAction = value;
        });
        actionExecuted.Should().BeTrue();
        valuePassedToAction.Should().Be("present");
    }

    [Test]
    public void IfPresent_WithoutValue_DoesNotExecuteAction()
    {
        var optional = Optional<string>.Empty();
        var actionExecuted = false;
        optional.IfPresent(_ => actionExecuted = true);
        actionExecuted.Should().BeFalse();
    }

    [Test]
    public async Task IfPresentAsync_WithValue_ExecutesAsyncAction()
    {
        var optional = Optional<int>.Of(42);
        var actionExecuted = false;
        int valuePassedToAction = 0;
        await optional.IfPresentAsync(async value =>
        {
            await Task.Delay(10);
            actionExecuted = true;
            valuePassedToAction = value;
        });
        actionExecuted.Should().BeTrue();
        valuePassedToAction.Should().Be(42);
    }

    [Test]
    public async Task IfPresentAsync_WithoutValue_DoesNotExecuteAsyncAction()
    {
        var optional = Optional<int>.Empty();
        var actionExecuted = false;
        await optional.IfPresentAsync(async _ =>
        {
            await Task.Delay(10);
            actionExecuted = true;
        });
        actionExecuted.Should().BeFalse();
    }

    [Test]
    public void IfNotPresent_WithoutValue_ExecutesAction()
    {
        var optional = Optional<string>.Empty();
        var actionExecuted = false;
        optional.IfNotPresent(() => actionExecuted = true);
        actionExecuted.Should().BeTrue();
    }

    [Test]
    public void IfNotPresent_WithValue_DoesNotExecuteAction()
    {
        var optional = Optional<string>.Of("present");
        var actionExecuted = false;
        optional.IfNotPresent(() => actionExecuted = true);
        actionExecuted.Should().BeFalse();
    }

    [Test]
    public async Task IfNotPresentAsync_WithoutValue_ExecutesAsyncAction()
    {
        var optional = Optional<int>.Empty();
        var actionExecuted = false;
        await optional.IfNotPresentAsync(async () =>
        {
            await Task.Delay(10);
            actionExecuted = true;
        });
        actionExecuted.Should().BeTrue();
    }

    [Test]
    public async Task IfNotPresentAsync_WithValue_DoesNotExecuteAsyncAction()
    {
        var optional = Optional<int>.Of(100);
        var actionExecuted = false;
        await optional.IfNotPresentAsync(async () =>
        {
            await Task.Delay(10);
            actionExecuted = true;
        });
        actionExecuted.Should().BeFalse();
    }

    [Test]
    public void ToString_WithValue_ReturnsStringRepresentationOfValue()
    {
        var optional = Optional<int>.Of(99);
        optional.ToString().Should().Be("99");
    }

    [Test]
    public void ToString_WithoutValue_ReturnsEmptyString()
    {
        var optional = Optional<string>.Empty();
        optional.ToString().Should().Be("Empty");
    }

    [Test]
    public void IfPresentOrElse_WithValue_ExecutesIfPresentAction()
    {
        var optional = Optional<int>.Of(5);
        var ifPresentExecuted = false;
        var orElseExecuted = false;

        optional.IfPresent(_ => ifPresentExecuted = true).OrElse(() => orElseExecuted = true);

        ifPresentExecuted.Should().BeTrue();
        orElseExecuted.Should().BeFalse();
    }

    [Test]
    public void IfPresentOrElse_WithoutValue_ExecutesOrElseAction()
    {
        var optional = Optional<int>.Empty();
        var ifPresentExecuted = false;
        var orElseExecuted = false;

        optional.IfPresent(_ => ifPresentExecuted = true).OrElse(() => orElseExecuted = true);

        ifPresentExecuted.Should().BeFalse();
        orElseExecuted.Should().BeTrue();
    }

    [Test]
    public void IfNotPresentOrElse_WithoutValue_ExecutesIfNotPresentAction()
    {
        var optional = Optional<int>.Empty();
        var ifNotPresentExecuted = false;
        var orElseExecuted = false;

        optional.IfNotPresent(() => ifNotPresentExecuted = true).OrElse(_ => orElseExecuted = true);

        ifNotPresentExecuted.Should().BeTrue();
        orElseExecuted.Should().BeFalse();
    }

    [Test]
    public void IfNotPresentOrElse_WithValue_ExecutesOrElseAction()
    {
        var optional = Optional<int>.Of(10);
        var ifNotPresentExecuted = false;
        var orElseExecuted = false;

        optional.IfNotPresent(() => ifNotPresentExecuted = true).OrElse(_ => orElseExecuted = true);

        ifNotPresentExecuted.Should().BeFalse();
        orElseExecuted.Should().BeTrue();
    }
}
