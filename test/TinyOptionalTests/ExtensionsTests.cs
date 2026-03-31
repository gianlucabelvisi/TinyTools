using FluentAssertions;
using TinyOptional;

namespace TinyOptionalTests;

public class ExtensionsTests
{
    [Test]
    public void FirstIfExists_WithNullSource_ReturnsEmptyOptional()
    {
        IEnumerable<int>? source = null;
        var result = source.FirstIfExists();
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void FirstIfExists_WithEmptySource_ReturnsEmptyOptional()
    {
        IEnumerable<int> source = new List<int>();
        var result = source.FirstIfExists();
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void FirstIfExists_WithNonEmptySource_ReturnsOptionalWithValue()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.FirstIfExists();
        result.Should().NotBeNull();
        result.IsPresent().Should().BeTrue();
        result.Get().Should().Be(1);
    }

    [Test]
    public void ElementAtIfExists_WithNullSource_ReturnsEmptyOptional()
    {
        IEnumerable<int>? source = null;
        var result = source.ElementAtIfExists(1);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void ElementAtIfExists_WithNegativeIndex_ReturnsEmptyOptional()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.ElementAtIfExists(-1);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void ElementAtIfExists_WithIndexOutOfRange_ReturnsEmptyOptional()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.ElementAtIfExists(3);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void ElementAtIfExists_WithValidIndex_ReturnsOptionalWithValue()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.ElementAtIfExists(1);
        result.Should().NotBeNull();
        result.IsPresent().Should().BeTrue();
        result.Get().Should().Be(2);
    }

    [Test]
    public void AggregateIfExists_WithNullSource_ReturnsEmptyOptional()
    {
        IEnumerable<int>? source = null;
        var result = source.AggregateIfExists(0, (acc, x) => acc + x);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void AggregateIfExists_WithEmptySource_ReturnsEmptyOptional()
    {
        IEnumerable<int> source = new List<int>();
        var result = source.AggregateIfExists(0, (acc, x) => acc + x);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void AggregateIfExists_WithNonEmptySource_ReturnsOptionalWithValue()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.AggregateIfExists(0, (acc, x) => acc + x);
        result.Should().NotBeNull();
        result.IsPresent().Should().BeTrue();
        result.Get().Should().Be(6);
    }

    [Test]
    public void LastIfExists_WithNullSource_ReturnsEmptyOptional()
    {
        IEnumerable<int>? source = null;
        var result = source.LastIfExists();
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void LastIfExists_WithEmptySource_ReturnsEmptyOptional()
    {
        IEnumerable<int> source = new List<int>();
        var result = source.LastIfExists();
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void LastIfExists_WithNonEmptySource_ReturnsOptionalWithValue()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.LastIfExists();
        result.Should().NotBeNull();
        result.IsPresent().Should().BeTrue();
        result.Get().Should().Be(3);
    }

    [Test]
    public void FirstIfExists_WithPredicate_WithNullSource_ReturnsEmptyOptional()
    {
        IEnumerable<int>? source = null;
        var result = source.FirstIfExists(x => x > 1);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void FirstIfExists_WithPredicate_WithEmptySource_ReturnsEmptyOptional()
    {
        IEnumerable<int> source = new List<int>();
        var result = source.FirstIfExists(x => x > 1);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void FirstIfExists_WithPredicate_WithMatchingElement_ReturnsOptionalWithValue()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.FirstIfExists(x => x > 1);
        result.Should().NotBeNull();
        result.IsPresent().Should().BeTrue();
        result.Get().Should().Be(2);
    }

    [Test]
    public void FirstIfExists_WithPredicate_WithNoMatchingElement_ReturnsEmptyOptional()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.FirstIfExists(x => x > 3);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void LastIfExists_WithPredicate_WithNullSource_ReturnsEmptyOptional()
    {
        IEnumerable<int>? source = null;
        var result = source.LastIfExists(x => x > 1);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void LastIfExists_WithPredicate_WithEmptySource_ReturnsEmptyOptional()
    {
        IEnumerable<int> source = new List<int>();
        var result = source.LastIfExists(x => x > 1);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void LastIfExists_WithPredicate_WithMatchingElement_ReturnsOptionalWithValue()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.LastIfExists(x => x > 1);
        result.Should().NotBeNull();
        result.IsPresent().Should().BeTrue();
        result.Get().Should().Be(3);
    }

    [Test]
    public void LastIfExists_WithPredicate_WithNoMatchingElement_ReturnsEmptyOptional()
    {
        IEnumerable<int> source = new List<int> { 1, 2, 3 };
        var result = source.LastIfExists(x => x > 3);
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void IfAny_WithNullString_ReturnsEmptyOptional()
    {
        string? str = null;
        var result = str.IfAny();
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void IfAny_WithEmptyString_ReturnsEmptyOptional()
    {
        string str = "";
        var result = str.IfAny();
        result.Should().NotBeNull();
        result.IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void IfAny_WithNonEmptyString_ReturnsOptionalWithValue()
    {
        string str = "test";
        var result = str.IfAny();
        result.Should().NotBeNull();
        result.IsPresent().Should().BeTrue();
        result.Get().Should().Be("test");
    }

    [Test]
    public void SingleIfExists_WithNullSource_ReturnsEmpty()
    {
        IEnumerable<int>? source = null;
        source.SingleIfExists().IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void SingleIfExists_WithEmptySource_ReturnsEmpty()
    {
        new List<int>().SingleIfExists().IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void SingleIfExists_WithOneElement_ReturnsOptionalWithValue()
    {
        new List<int> { 42 }.SingleIfExists().Get().Should().Be(42);
    }

    [Test]
    public void SingleIfExists_WithMultipleElements_ReturnsEmpty()
    {
        new List<int> { 1, 2 }.SingleIfExists().IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void SingleIfExists_WithPredicate_WithOneMatch_ReturnsOptionalWithValue()
    {
        new List<int> { 1, 2, 3 }.SingleIfExists(x => x > 2).Get().Should().Be(3);
    }

    [Test]
    public void SingleIfExists_WithPredicate_WithNoMatch_ReturnsEmpty()
    {
        new List<int> { 1, 2, 3 }.SingleIfExists(x => x > 10).IsNotPresent().Should().BeTrue();
    }

    [Test]
    public void SingleIfExists_WithPredicate_WithMultipleMatches_ReturnsEmpty()
    {
        new List<int> { 1, 2, 3 }.SingleIfExists(x => x > 1).IsNotPresent().Should().BeTrue();
    }
}
