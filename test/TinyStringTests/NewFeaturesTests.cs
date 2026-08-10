using AwesomeAssertions;
using TinyString;

namespace TinyStringTests;

[TestFixture]
public class NewFeaturesTests
{
    public class Animal
    {
        public required string Name { get; set; }
        public required string Species { get; set; }
        public int Age { get; set; }
    }

    // ── Runtime-type reflection (bug fix) ───────────────────────────────────

    [Test]
    public void Stringify_ThroughBaseReference_UsesRuntimeType()
    {
        object animal = new Animal { Name = "Mittens", Species = "Cat", Age = 5 };

        // Called as object — previously reflected typeof(object) and produced just "Object".
        var result = animal.Stringify();

        result.Should().Contain("Name: Mittens");
        result.Should().Contain("Species: Cat");
        result.Should().StartWith("Animal.");
    }

    // ── Stringify(options) overload ─────────────────────────────────────────

    [Test]
    public void Stringify_WithPrebuiltOptions_IsReusable()
    {
        var options = new StringifyOptions<Animal>().NoLabel();
        options.For(x => x.Age).Ignore();

        var a = new Animal { Name = "Mittens", Species = "Cat", Age = 5 };
        var b = new Animal { Name = "Tony", Species = "Tiger", Age = 6 };

        a.Stringify(options).Should().Be("Name: Mittens, Species: Cat");
        b.Stringify(options).Should().Be("Name: Tony, Species: Tiger");
    }

    [Test]
    public void Stringify_WithNullOptions_Throws()
    {
        var a = new Animal { Name = "Mittens", Species = "Cat", Age = 5 };
        Action act = () => a.Stringify((StringifyOptions<Animal>)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Format (general IFormattable) ───────────────────────────────────────

    public class Widget
    {
        public Guid Id { get; set; }
        public TimeSpan Duration { get; set; }
        public double Ratio { get; set; }
    }

    [Test]
    public void Format_FormatsTimeSpan()
    {
        var w = new Widget { Duration = new TimeSpan(1, 2, 3) };

        var result = w.Stringify(o => o
            .NoLabel()
            .Only(x => x.Duration)
            .For(x => x.Duration).Format(@"hh\:mm\:ss"));

        result.Should().Be("Duration: 01:02:03");
    }

    [Test]
    public void Format_FormatsDouble()
    {
        var w = new Widget { Ratio = 0.1234 };

        var result = w.Stringify(o => o
            .NoLabel()
            .Only(x => x.Ratio)
            .For(x => x.Ratio).Format("P1"));

        result.Should().Be("Ratio: 12.3 %");
    }

    // ── MaxItems(0) guard ───────────────────────────────────────────────────

    public class Bag
    {
        public List<int> Items { get; set; } = [];
    }

    [Test]
    public void MaxItems_Zero_ShowsOnlyOverflowMarker()
    {
        var bag = new Bag { Items = [1, 2, 3] };

        var result = bag.Stringify(o => o
            .NoLabel()
            .For(x => x.Items).MaxItems(0));

        result.Should().Contain("... and 3 more");
        result.Should().NotContain("1, 2, 3");
    }

    // ── StringExtensions ────────────────────────────────────────────────────

    [Test]
    public void Truncate_AddsEllipsis_WithinMaxLength()
    {
        "Hello, World".Truncate(8).Should().Be("Hello, …");
        "Hello, World".Truncate(8).Length.Should().Be(8);
    }

    [Test]
    public void Truncate_LeavesShortStrings()
    {
        "Hi".Truncate(8).Should().Be("Hi");
    }

    [Test]
    public void Repeat_RepeatsString()
    {
        "ab".Repeat(3).Should().Be("ababab");
        "ab".Repeat(0).Should().Be("");
    }

    [Test]
    public void ToTitleCase_CapitalisesEachWord()
    {
        "hello wORLD".ToTitleCase().Should().Be("Hello World");
    }

    [Test]
    public void EnsurePrefix_And_EnsureSuffix()
    {
        "value".EnsurePrefix("--").Should().Be("--value");
        "--value".EnsurePrefix("--").Should().Be("--value");
        "file".EnsureSuffix(".txt").Should().Be("file.txt");
        "file.txt".EnsureSuffix(".txt").Should().Be("file.txt");
    }

    [Test]
    public void NullIfEmpty_ReturnsNullForEmpty()
    {
        "".NullIfEmpty().Should().BeNull();
        ((string?)null).NullIfEmpty().Should().BeNull();
        "x".NullIfEmpty().Should().Be("x");
    }

    [Test]
    public void Reverse_ReversesCharacters()
    {
        "abc".Reverse().Should().Be("cba");
        "".Reverse().Should().Be("");
    }
}
