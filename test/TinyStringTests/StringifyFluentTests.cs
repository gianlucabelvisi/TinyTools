using FluentAssertions;
using TinyString;

namespace TinyStringTests;

/// <summary>
/// Tests for the fluent Stringify API.
/// None of these classes carry [Stringify] attributes — all configuration
/// is expressed at the call site.
/// </summary>
[TestFixture]
public class StringifyFluentTests
{
    // ── Domain model (no attributes) ────────────────────────────────────────

    public class Animal
    {
        public required string Name { get; set; }
        public required string Species { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }
        public bool IsRare { get; set; }
    }

    public class Zoo
    {
        public required string Name { get; set; }
        public List<Animal> Animals { get; set; } = [];
        public double EntrancePrice { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public double TaxRate { get; set; }
        public string InternalRef { get; set; } = "";
    }

    // ── Zero-config defaults ────────────────────────────────────────────────

    [Test]
    public void ZeroConfig_ShowsClassName_ThenProperties()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.567, Age = 5, IsRare = false };

        animal.Stringify().Should().Be(
            "Animal. Name: Mittens, Species: Cat, Weight: 4.57, Age: 5, IsRare: False");
    }

    [Test]
    public void ZeroConfig_Decimals_DefaultsToTwo()
    {
        var animal = new Animal { Name = "X", Species = "Y", Weight = 1.23456, Age = 0, IsRare = false };

        animal.Stringify().Should().Contain("Weight: 1.23");
        animal.Stringify().Should().NotContain("1.23456");
    }

    // ── Label / header ──────────────────────────────────────────────────────

    [Test]
    public void Label_OverridesClassName()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        animal.Stringify(o => o.Label("🐾 Creature")).Should().StartWith("🐾 Creature. ");
    }

    [Test]
    public void NoLabel_HidesHeader()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o.NoLabel());

        result.Should().NotContain("Animal");
        result.Should().StartWith("Name: Mittens");
    }

    // ── Single-line separator ───────────────────────────────────────────────

    [Test]
    public void Separator_OverridesPropertyDelimiter()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o.NoLabel().Separator(" | "));

        result.Should().Contain("Name: Mittens | Species: Cat");
    }

    // ── MultiLine ───────────────────────────────────────────────────────────

    [Test]
    public void MultiLine_PrintsEachPropertyOnItsOwnLine()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o.MultiLine());
        var lines = result!.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        lines[0].Trim().Should().Be("Animal");
        lines.Should().Contain(l => l.Trim() == "Name: Mittens");
        lines.Should().Contain(l => l.Trim() == "Species: Cat");
    }

    // ── Property: Ignore ────────────────────────────────────────────────────

    [Test]
    public void For_Ignore_ExcludesProperty()
    {
        var order = new Order { Id = 1, Amount = 99.9, TaxRate = 0.2, InternalRef = "secret" };

        var result = order.Stringify(o => o
            .For(x => x.InternalRef).Ignore()
            .For(x => x.TaxRate).Ignore());

        result.Should().Contain("Id: 1");
        result.Should().Contain("Amount:");
        result.Should().NotContain("InternalRef");
        result.Should().NotContain("TaxRate");
        result.Should().NotContain("secret");
    }

    // ── Property: NoKey ─────────────────────────────────────────────────────

    [Test]
    public void For_NoKey_ShowsOnlyValue()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o
            .NoLabel()
            .Separator(" ")
            .For(x => x.Name).NoKey()
            .For(x => x.Species).NoKey()
            .For(x => x.Weight).Ignore()
            .For(x => x.Age).Ignore()
            .For(x => x.IsRare).Ignore());

        result.Should().Be("Mittens Cat");
    }

    // ── Property: Prefix / Suffix ───────────────────────────────────────────

    [Test]
    public void For_Prefix_AndSuffix_WrapValue()
    {
        var order = new Order { Id = 42, Amount = 99.99, TaxRate = 20.0, InternalRef = "REF" };

        var result = order.Stringify(o => o
            .NoLabel()
            .Separator(" | ")
            .For(x => x.Id).Ignore()
            .For(x => x.InternalRef).Ignore()
            .For(x => x.Amount).Prefix("$").NoKey()
            .For(x => x.TaxRate).Suffix("%").NoKey());

        result.Should().Be("$99.99 | 20.00%");
    }

    // ── Property: As (rename key) ───────────────────────────────────────────

    [Test]
    public void For_As_RenamesKey()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o
            .NoLabel()
            .For(x => x.Name).As("nom")
            .For(x => x.Species).As("espèce")
            .For(x => x.Weight).Ignore()
            .For(x => x.Age).Ignore()
            .For(x => x.IsRare).Ignore());

        result.Should().Be("nom: Mittens, espèce: Cat");
    }

    // ── Property: Decimals override ─────────────────────────────────────────

    [Test]
    public void For_Decimals_OverridesGlobalDecimals()
    {
        var order = new Order { Id = 1, Amount = 9.9999, TaxRate = 0.2, InternalRef = "" };

        var result = order.Stringify(o => o
            .Decimals(0)
            .For(x => x.Amount).Decimals(4));

        result.Should().Contain("Amount: 9.9999");
        result.Should().Contain("TaxRate: 0");
    }

    // ── Collection separator ────────────────────────────────────────────────

    [Test]
    public void CollectionSeparator_JoinsItems()
    {
        var zoo = new Zoo
        {
            Name = "City Zoo",
            EntrancePrice = 10,
            Animals =
            [
                new Animal { Name = "Mittens", Species = "Cat",   Weight = 4.5,   Age = 5, IsRare = false },
                new Animal { Name = "Tony",    Species = "Tiger",  Weight = 120.3, Age = 6, IsRare = true },
            ]
        };

        var result = zoo.Stringify(o => o
            .NoLabel()
            .For(x => x.Animals).Separator(" | ")
            .For(x => x.EntrancePrice).Ignore());

        result.Should().Contain(" | ");
    }

    // ── Keys naming format ──────────────────────────────────────────────────

    [Test]
    public void Keys_HumanCase_InsertsSpaces()
    {
        var animal = new Animal { Name = "X", Species = "Y", Weight = 1, Age = 1, IsRare = true };

        var result = animal.Stringify(o => o.NoLabel().Keys(NamingFormat.HumanCase));

        result.Should().Contain("Is Rare: True");
    }

    [Test]
    public void Keys_SnakeCase_UsesUnderscores()
    {
        var animal = new Animal { Name = "X", Species = "Y", Weight = 1, Age = 1, IsRare = false };

        var result = animal.Stringify(o => o.NoLabel().Keys(NamingFormat.SnakeCase));

        result.Should().Contain("is_rare: False");
    }

    // ── Chaining: multiple For() calls ──────────────────────────────────────

    [Test]
    public void MultipleFor_CanChain_FromPropertyBuilder()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        // Each .For() is called on the PropertyBuilder of the previous one — no need
        // to return to StringifyOptions in between.
        var result = animal.Stringify(o => o
            .NoLabel()
            .Separator(" ")
            .For(x => x.Name).NoKey()
            .For(x => x.Species).Prefix("(").Suffix(")").NoKey()
            .For(x => x.Weight).NoKey().Suffix("kg").Decimals(2)
            .For(x => x.Age).NoKey().Suffix("yrs")
            .For(x => x.IsRare).Ignore());

        result.Should().Be("Mittens (Cat) 4.50kg 5yrs");
    }

    // ── Default collection rendering by style ──────────────────────────────

    [Test]
    public void SingleLine_Collection_DefaultsToComma()
    {
        var zoo = new Zoo
        {
            Name = "City Zoo",
            EntrancePrice = 10,
            Animals =
            [
                new Animal { Name = "Mittens", Species = "Cat",  Weight = 4.5,   Age = 5, IsRare = false },
                new Animal { Name = "Tony",    Species = "Tiger", Weight = 120.3, Age = 6, IsRare = true  },
            ]
        };

        var result = zoo.Stringify(o => o.NoLabel().For(x => x.EntrancePrice).Ignore());

        // Items joined with ", " — no |_ prefix in single-line mode
        result.Should().Contain("Animals: ");
        result.Should().NotContain("|_");
    }

    [Test]
    public void MultiLine_Collection_DefaultsToListPrefix()
    {
        var zoo = new Zoo
        {
            Name = "Wonderful Zoo",
            EntrancePrice = 14.99,
            Animals =
            [
                new Animal { Name = "Mittens", Species = "Cat",     Weight = 4.5,   Age = 5,  IsRare = false },
                new Animal { Name = "Tony",    Species = "Tiger",    Weight = 120.3, Age = 6,  IsRare = true  },
                new Animal { Name = "Dumbo",   Species = "Elephant", Weight = 500.1, Age = 10, IsRare = false },
            ]
        };

        // No .For(x => x.Animals) needed — |_ is the automatic default in multi-line mode
        var result = zoo.Stringify(o => o.MultiLine());

        result.Should().Contain("|_ ");
        var lines = result!.Split('\n');
        lines.Count(l => l.StartsWith("|_ ")).Should().Be(3);
    }

    [Test]
    public void Zoo_MultiLine_WithLabel()
    {
        var zoo = new Zoo
        {
            Name = "Wonderful Zoo",
            EntrancePrice = 14.99,
            Animals =
            [
                new Animal { Name = "Mittens", Species = "Cat",      Weight = 4.5,   Age = 5,  IsRare = false },
                new Animal { Name = "Tony",    Species = "Tiger",     Weight = 120.3, Age = 6,  IsRare = true  },
                new Animal { Name = "Dumbo",   Species = "Elephant",  Weight = 500.1, Age = 10, IsRare = false },
            ]
        };

        var result = zoo.Stringify(o => o
            .MultiLine()
            .Label("🦁🦓🦍 Zoo")
            .For(x => x.EntrancePrice).Prefix("$").Decimals(0));

        result.Should().StartWith("🦁🦓🦍 Zoo");
        result.Should().Contain("Name: Wonderful Zoo");
        result.Should().Contain("EntrancePrice: $15");
        result.Should().Contain("|_ ");
    }

    // ── Null handling ───────────────────────────────────────────────────────

    [Test]
    public void NullObject_ReturnsNull()
    {
        Animal? animal = null;
        animal.Stringify().Should().BeNull();
    }
}
