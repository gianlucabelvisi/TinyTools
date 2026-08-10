using AwesomeAssertions;
using TinyOptional;

namespace TinyOptionalTests;

public class NewFeaturesTests
{
    // ── operator == / != null-safety (bug fix) ──────────────────────────────

    [Test]
    public void EqualityOperator_WithNullLeftOperand_DoesNotThrow()
    {
        Optional<int>? left = null;
        var right = Optional<int>.Of(1);

        (left == right).Should().BeFalse();
        (right == left).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Test]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        Optional<int>? left = null;
        Optional<int>? right = null;

        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
    }

    // ── null-argument guards ────────────────────────────────────────────────

    [Test]
    public void Where_WithNullPredicate_Throws()
    {
        var optional = Optional<int>.Of(1);
        Action act = () => optional.Where(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Match_WithNullHandler_Throws()
    {
        var optional = Optional<int>.Of(1);
        Action act = () => optional.Match<int>(null!, () => 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void OrElseGet_WithNullSupplier_Throws()
    {
        var optional = Optional<int>.Empty();
        Action act = () => optional.OrElseGet(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Or ──────────────────────────────────────────────────────────────────

    [Test]
    public void Or_WithPresentValue_ReturnsSelf()
    {
        Optional<int>.Of(1).Or(Optional<int>.Of(2)).Get().Should().Be(1);
    }

    [Test]
    public void Or_WithEmpty_ReturnsFallback()
    {
        Optional<int>.Empty().Or(Optional<int>.Of(2)).Get().Should().Be(2);
    }

    [Test]
    public void Or_Supplier_NotInvoked_WhenPresent()
    {
        var invoked = false;
        var result = Optional<int>.Of(1).Or(() => { invoked = true; return Optional<int>.Of(2); });
        invoked.Should().BeFalse();
        result.Get().Should().Be(1);
    }

    [Test]
    public void Or_Supplier_Invoked_WhenEmpty()
    {
        var result = Optional<int>.Empty().Or(() => Optional<int>.Of(42));
        result.Get().Should().Be(42);
    }

    // ── OfType ──────────────────────────────────────────────────────────────

    [Test]
    public void OfType_WithAssignableValue_ReturnsTyped()
    {
        object boxed = "hello";
        Optional<object>.Of(boxed).OfType<string>().Get().Should().Be("hello");
    }

    [Test]
    public void OfType_WithIncompatibleValue_ReturnsEmpty()
    {
        object boxed = 5;
        Optional<object>.Of(boxed).OfType<string>().IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void OfType_WhenEmpty_ReturnsEmpty()
    {
        Optional<object>.Empty().OfType<string>().IsNotPresent().Should().BeTrue();
    }

    // ── Tap ─────────────────────────────────────────────────────────────────

    [Test]
    public void Tap_WithValue_ExecutesAndReturnsSelf()
    {
        var seen = 0;
        var result = Optional<int>.Of(7).Tap(v => seen = v);
        seen.Should().Be(7);
        result.Get().Should().Be(7);
    }

    [Test]
    public void Tap_WhenEmpty_DoesNotExecute()
    {
        var executed = false;
        Optional<int>.Empty().Tap(_ => executed = true);
        executed.Should().BeFalse();
    }

    // ── ToOptional / ToNullable / Flatten ───────────────────────────────────

    [Test]
    public void ToOptional_ReferenceType_WrapsValue()
    {
        "x".ToOptional().Get().Should().Be("x");
        ((string?)null).ToOptional().IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void ToOptional_NullableValueType_WrapsValue()
    {
        int? some = 5;
        int? none = null;
        some.ToOptional().Get().Should().Be(5);
        none.ToOptional().IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void ToNullable_RoundTrips()
    {
        Optional<int>.Of(3).ToNullable().Should().Be(3);
        Optional<int>.Empty().ToNullable().Should().BeNull();
    }

    [Test]
    public void Flatten_CollapsesNestedOptional()
    {
        Optional<Optional<int>>.Of(Optional<int>.Of(9)).Flatten().Get().Should().Be(9);
        Optional<Optional<int>>.Of(Optional<int>.Empty()).Flatten().IsNotPresent().Should().BeTrue();
        Optional<Optional<int>>.Empty().Flatten().IsNotPresent().Should().BeTrue();
    }

    // ── Values ──────────────────────────────────────────────────────────────

    [Test]
    public void Values_ProjectsPresentValuesOnly()
    {
        var seq = new[]
        {
            Optional<int>.Of(1),
            Optional<int>.Empty(),
            Optional<int>.Of(3),
        };

        seq.Values().Should().Equal(1, 3);
    }

    // ── Min / Max ────────────────────────────────────────────────────────────

    [Test]
    public void MinIfExists_ReturnsMinimum()
    {
        new[] { 3, 1, 2 }.MinIfExists().Get().Should().Be(1);
    }

    [Test]
    public void MaxIfExists_ReturnsMaximum()
    {
        new[] { 3, 1, 2 }.MaxIfExists().Get().Should().Be(3);
    }

    [Test]
    public void MinMaxIfExists_EmptyOrNull_ReturnsEmpty()
    {
        Array.Empty<int>().MinIfExists().IsNotPresent().Should().BeTrue();
        ((IEnumerable<int>?)null).MaxIfExists().IsNotPresent().Should().BeTrue();
    }

    // ── Null-element tolerance (bug fix) ────────────────────────────────────

    [Test]
    public void FirstIfExists_WithNullFirstElement_ReturnsEmpty_NotThrows()
    {
        var source = new List<string?> { null, "b" };
        Action act = () => source.FirstIfExists();
        act.Should().NotThrow();
        source.FirstIfExists().IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void LastIfExists_WithNullLastElement_ReturnsEmpty_NotThrows()
    {
        var source = new List<string?> { "a", null };
        Action act = () => source.LastIfExists();
        act.Should().NotThrow();
        source.LastIfExists().IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void ElementAtIfExists_WithNullElement_ReturnsEmpty_NotThrows()
    {
        var source = new List<string?> { "a", null, "c" };
        source.ElementAtIfExists(1).IsNotPresent().Should().BeTrue();
        source.ElementAtIfExists(2).Get().Should().Be("c");
    }

    [Test]
    public void SingleIfExists_WithNullElement_ReturnsEmpty_NotThrows()
    {
        var source = new List<string?> { null };
        Action act = () => source.SingleIfExists();
        act.Should().NotThrow();
        source.SingleIfExists().IsNotPresent().Should().BeTrue();
    }
}
