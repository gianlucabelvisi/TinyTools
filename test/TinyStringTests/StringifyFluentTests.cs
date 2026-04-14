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

    // ── Only ────────────────────────────────────────────────────────────────

    [Test]
    public void Only_ShowsWhitelistedPropertiesAndExcludesRest()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o.NoLabel().Only(x => x.Name, x => x.Species));

        result.Should().Be("Name: Mittens, Species: Cat");
        result.Should().NotContain("Weight");
        result.Should().NotContain("Age");
        result.Should().NotContain("IsRare");
    }

    [Test]
    public void Only_WorksWithValueTypeProperties()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        // Age (int) and IsRare (bool) are value types — make sure boxing in expressions is handled
        var result = animal.Stringify(o => o.NoLabel().Only(x => x.Age, x => x.IsRare));

        result.Should().Contain("Age: 5");
        result.Should().Contain("IsRare: False");
        result.Should().NotContain("Name");
        result.Should().NotContain("Species");
    }

    // ── ValueFormat ─────────────────────────────────────────────────────────

    [Test]
    public void ValueFormat_ReplacesDefaultConversion()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.567, Age = 5, IsRare = true };

        var result = animal.Stringify(o => o
            .NoLabel()
            .For(x => x.Weight).ValueFormat(w => w.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + " kg")
            .For(x => x.IsRare).ValueFormat(r => r ? "⭐ Rare" : "Common")
            .For(x => x.Name).Ignore()
            .For(x => x.Species).Ignore()
            .For(x => x.Age).Ignore());

        result.Should().Contain("Weight: 4.6 kg");
        result.Should().Contain("IsRare: ⭐ Rare");
    }

    // ── When ────────────────────────────────────────────────────────────────

    [Test]
    public void When_HidesProperty_WhenPredicateIsFalse()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o.NoLabel().For(x => x.IsRare).When(v => v));

        result.Should().NotContain("IsRare");
    }

    [Test]
    public void When_ShowsProperty_WhenPredicateIsTrue()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = true };

        var result = animal.Stringify(o => o.NoLabel().For(x => x.IsRare).When(v => v));

        result.Should().Contain("IsRare: True");
    }

    [Test]
    public void When_CanFilterCollectionProperties()
    {
        var zoo = new Zoo
        {
            Name = "Empty Zoo",
            EntrancePrice = 10,
            Animals = []
        };

        // Only show Animals when the list is non-empty
        var result = zoo.Stringify(o => o
            .NoLabel()
            .For(x => x.Animals).When(a => a.Count > 0)
            .For(x => x.EntrancePrice).Ignore());

        result.Should().NotContain("Animals");
    }

    // ── NullAs ──────────────────────────────────────────────────────────────

    public class Sparse
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Count { get; set; }
    }

    [Test]
    public void NullAs_OverridesNullToken()
    {
        var sparse = new Sparse { Name = "Test", Description = null, Count = 3 };

        var result = sparse.Stringify(o => o.NoLabel().NullAs("—"));

        result.Should().Contain("Description: —");
        result.Should().NotContain("null");
    }

    // ── MaxItems ────────────────────────────────────────────────────────────

    [Test]
    public void MaxItems_TruncatesCollection_AndShowsOverflow()
    {
        var zoo = new Zoo
        {
            Name = "Big Zoo",
            EntrancePrice = 0,
            Animals =
            [
                new Animal { Name = "A", Species = "Cat",   Weight = 1, Age = 1, IsRare = false },
                new Animal { Name = "B", Species = "Cat",   Weight = 1, Age = 1, IsRare = false },
                new Animal { Name = "C", Species = "Dog",   Weight = 2, Age = 2, IsRare = false },
                new Animal { Name = "D", Species = "Tiger", Weight = 3, Age = 3, IsRare = true  },
                new Animal { Name = "E", Species = "Lion",  Weight = 4, Age = 4, IsRare = true  },
            ]
        };

        var result = zoo.Stringify(o => o
            .NoLabel()
            .For(x => x.Animals).MaxItems(3)
            .For(x => x.EntrancePrice).Ignore());

        result.Should().Contain("... and 2 more");
        result.Should().NotContain("Name: D");
        result.Should().NotContain("Name: E");
    }

    [Test]
    public void MaxItems_NoTruncation_WhenCountWithinLimit()
    {
        var zoo = new Zoo
        {
            Name = "Small Zoo",
            EntrancePrice = 0,
            Animals =
            [
                new Animal { Name = "A", Species = "Cat", Weight = 1, Age = 1, IsRare = false },
                new Animal { Name = "B", Species = "Dog", Weight = 2, Age = 2, IsRare = false },
            ]
        };

        var result = zoo.Stringify(o => o
            .NoLabel()
            .For(x => x.Animals).MaxItems(5)
            .For(x => x.EntrancePrice).Ignore());

        result.Should().NotContain("more");
    }

    // ── Multi-line indentation ──────────────────────────────────────────────

    [Test]
    public void MultiLine_NestedForNested_IndentsNestedLines()
    {
        var zoo = new Zoo
        {
            Name = "Indent Zoo",
            EntrancePrice = 0,
            Animals =
            [
                new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false },
                new Animal { Name = "Tony",    Species = "Tiger", Weight = 120, Age = 6, IsRare = true },
            ]
        };

        var result = zoo.Stringify(o => o
            .MultiLine()
            .NoLabel()
            .ForNested<Animal>(a => a
                .MultiLine()
                .NoLabel()
                .For(x => x.Weight).Ignore()
                .For(x => x.Age).Ignore()
                .For(x => x.IsRare).Ignore())
            .For(x => x.EntrancePrice).Ignore());

        var lines = result!.Split('\n');

        // Each animal starts with |_
        lines.Should().Contain(l => l.StartsWith("|_ Name: Mittens"));
        // Subsequent property lines of the nested animal are indented by 3 spaces ("|_ " width)
        lines.Should().Contain(l => l.StartsWith("   Species: Cat"));
        lines.Should().Contain(l => l.StartsWith("   Species: Tiger"));
    }

    // ── ToString() override ─────────────────────────────────────────────────

    public class AnimalWithToString
    {
        public required string Name { get; set; }
        public required string Species { get; set; }
        public override string ToString() => $"{Name} ({Species})";
    }

    public class ZooWithToStringAnimals
    {
        public required string Name { get; set; }
        public List<AnimalWithToString> Animals { get; set; } = [];
    }

    [Test]
    public void NestedObject_UsesToString_WhenOverridden()
    {
        var zoo = new ZooWithToStringAnimals
        {
            Name = "City Zoo",
            Animals =
            [
                new AnimalWithToString { Name = "Mittens", Species = "Cat" },
                new AnimalWithToString { Name = "Tony",    Species = "Tiger" },
            ]
        };

        var result = zoo.Stringify(o => o.MultiLine());

        // Each animal should be rendered via its ToString(), not via reflection
        result.Should().Contain("Mittens (Cat)");
        result.Should().Contain("Tony (Tiger)");
        result.Should().NotContain("Name: Mittens");
    }

    // ── ForNested ───────────────────────────────────────────────────────────

    [Test]
    public void ForNested_ConfiguresNestedType_WhereverItAppears()
    {
        var zoo = new Zoo
        {
            Name = "Woodland Zoo",
            EntrancePrice = 14.99,
            Animals =
            [
                new Animal { Name = "Mittens", Species = "Cat",   Weight = 4.5,   Age = 5, IsRare = false },
                new Animal { Name = "Tony",    Species = "Tiger",  Weight = 120.3, Age = 6, IsRare = true  },
            ]
        };

        var result = zoo.Stringify(o => o
            .MultiLine()
            .ForNested<Animal>(a => a
                .NoLabel()
                .Separator(" ")
                .For(x => x.Name).NoKey()
                .For(x => x.Species).Prefix("(").Suffix(")").NoKey()
                .For(x => x.Weight).NoKey().Suffix("kg").Decimals(2)
                .For(x => x.Age).NoKey().Suffix("yrs")
                .For(x => x.IsRare).Ignore()));

        result.Should().Contain("|_ Mittens (Cat) 4.50kg 5yrs");
        result.Should().Contain("|_ Tony (Tiger) 120.30kg 6yrs");
        result.Should().NotContain("Name: Mittens");
    }

    [Test]
    public void ForNested_CanChainWithFor()
    {
        var zoo = new Zoo
        {
            Name = "Tiny Zoo",
            EntrancePrice = 5,
            Animals = [ new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false } ]
        };

        // ForNested returns StringifyOptions<T> so further .For() calls work normally
        var result = zoo.Stringify(o => o
            .MultiLine()
            .ForNested<Animal>(a => a.NoLabel().For(x => x.IsRare).Ignore())
            .For(x => x.EntrancePrice).Prefix("$").Decimals(0));

        result.Should().Contain("EntrancePrice: $5");
        result.Should().NotContain("IsRare");
    }

    // ── Null handling ───────────────────────────────────────────────────────

    [Test]
    public void NullObject_ReturnsNull()
    {
        Animal? animal = null;
        animal.Stringify().Should().BeNull();
    }

    // ── ReplaceValues ──────────────────────────────────────────────────────

    public class Task
    {
        public required string Title { get; set; }
        public required string Source { get; set; }
        public int Priority { get; set; }
    }

    [Test]
    public void ReplaceValues_SubstitutesMatchingValues()
    {
        var task = new Task { Title = "Fix bug", Source = "inbox", Priority = 1 };

        var result = task.Stringify(o => o
            .NoLabel()
            .For(x => x.Source).ReplaceValues(new() { { "area", "🗺️" }, { "inbox", "📥" } })
            .For(x => x.Priority).Ignore());

        result.Should().Contain("Source: 📥");
    }

    [Test]
    public void ReplaceValues_FallsBackToNormalRendering_WhenNoMatch()
    {
        var task = new Task { Title = "Fix bug", Source = "other", Priority = 1 };

        var result = task.Stringify(o => o
            .NoLabel()
            .For(x => x.Source).ReplaceValues(new() { { "area", "🗺️" }, { "inbox", "📥" } })
            .For(x => x.Priority).Ignore());

        result.Should().Contain("Source: other");
    }

    public class TaskWithOwner
    {
        public required string Title { get; set; }
        public required AnimalWithToString Owner { get; set; }
    }

    [Test]
    public void ReplaceValues_MatchesToString_OnNestedObject()
    {
        var task = new TaskWithOwner
        {
            Title = "Fix bug",
            Owner = new AnimalWithToString { Name = "Mittens", Species = "Cat" }
        };

        var result = task.Stringify(o => o
            .NoLabel()
            .For(x => x.Owner).ReplaceValues(new() {
                { new AnimalWithToString { Name = "Mittens", Species = "Cat" }, "🐱 Mittens" },
                { new AnimalWithToString { Name = "Tony", Species = "Tiger" }, "🐯 Tony" }
            }));

        result.Should().Contain("Owner: 🐱 Mittens");
    }

    [Test]
    public void ReplaceValues_WorksWithIntegerKeys()
    {
        var task = new Task { Title = "Fix bug", Source = "inbox", Priority = 1 };

        var result = task.Stringify(o => o
            .NoLabel()
            .For(x => x.Priority).ReplaceValues(new() { { 1, "🔴 Critical" }, { 2, "🟡 Medium" } })
            .For(x => x.Source).Ignore());

        result.Should().Contain("Priority: 🔴 Critical");
    }

    // ── DateFormat ──────────────────────────────────────────────────────────

    public class Event
    {
        public required string Name { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    [Test]
    public void DateFormat_FormatsDateTime()
    {
        var ev = new Event
        {
            Name = "Launch",
            StartsAt = new DateTime(2026, 3, 15, 14, 30, 0),
            CreatedAt = DateTimeOffset.MinValue,
        };

        var result = ev.Stringify(o => o
            .NoLabel()
            .For(x => x.StartsAt).DateFormat("yyyy-MM-dd")
            .For(x => x.CreatedAt).Ignore());

        result.Should().Contain("StartsAt: 2026-03-15");
    }

    [Test]
    public void DateFormat_FormatsDateTimeOffset()
    {
        var ev = new Event
        {
            Name = "Launch",
            StartsAt = DateTime.MinValue,
            CreatedAt = new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero),
        };

        var result = ev.Stringify(o => o
            .NoLabel()
            .For(x => x.CreatedAt).DateFormat("dd MMM yyyy HH:mm")
            .For(x => x.StartsAt).Ignore());

        result.Should().Contain("CreatedAt: 01 Jun 2026 09:00");
    }

    // ── BoolAs ──────────────────────────────────────────────────────────────

    [Test]
    public void BoolAs_ReplacesTrue()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = true };

        var result = animal.Stringify(o => o
            .NoLabel()
            .Only(x => x.IsRare)
            .For(x => x.IsRare).BoolAs("✅", "❌"));

        result.Should().Be("IsRare: ✅");
    }

    [Test]
    public void BoolAs_ReplacesFalse()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o
            .NoLabel()
            .Only(x => x.IsRare)
            .For(x => x.IsRare).BoolAs("Yes", "No"));

        result.Should().Be("IsRare: No");
    }

    // ── Truncate ────────────────────────────────────────────────────────────

    [Test]
    public void Truncate_CutsLongStrings()
    {
        var animal = new Animal { Name = "Bartholomew McFluffington III", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o
            .NoLabel()
            .Only(x => x.Name)
            .For(x => x.Name).Truncate(10));

        result.Should().Be("Name: Bartholome…");
    }

    [Test]
    public void Truncate_LeavesShortStringsAlone()
    {
        var animal = new Animal { Name = "Kit", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o
            .NoLabel()
            .Only(x => x.Name)
            .For(x => x.Name).Truncate(10));

        result.Should().Be("Name: Kit");
    }

    // ── Transform / UpperCase / LowerCase ───────────────────────────────────

    [Test]
    public void Transform_AppliesCustomTransform()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o
            .NoLabel()
            .Only(x => x.Name)
            .For(x => x.Name).Transform(v => $"*** {v} ***"));

        result.Should().Be("Name: *** Mittens ***");
    }

    [Test]
    public void UpperCase_ConvertsToUpperCase()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o
            .NoLabel()
            .Only(x => x.Name)
            .For(x => x.Name).UpperCase());

        result.Should().Be("Name: MITTENS");
    }

    [Test]
    public void LowerCase_ConvertsToLowerCase()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o
            .NoLabel()
            .Only(x => x.Species)
            .For(x => x.Species).LowerCase());

        result.Should().Be("Species: cat");
    }

    // ── Per-property NullAs ─────────────────────────────────────────────────

    [Test]
    public void PropertyNullAs_OverridesGlobalNullToken()
    {
        var sparse = new Sparse { Name = null, Description = null, Count = 3 };

        var result = sparse.Stringify(o => o
            .NoLabel()
            .NullAs("—")
            .For(x => x.Name).NullAs("N/A"));

        result.Should().Contain("Name: N/A");
        result.Should().Contain("Description: —");
    }

    // ── NumberFormat ────────────────────────────────────────────────────────

    public class Invoice
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public double TaxRate { get; set; }
    }

    [Test]
    public void NumberFormat_FormatsWithThousandsSeparator()
    {
        var invoice = new Invoice { Id = 1, Amount = 12345.6789, TaxRate = 0.21 };

        var result = invoice.Stringify(o => o
            .NoLabel()
            .Only(x => x.Amount)
            .For(x => x.Amount).NumberFormat("N2"));

        // InvariantCulture uses comma as thousands separator
        result.Should().Be("Amount: 12,345.68");
    }

    [Test]
    public void NumberFormat_FormatsPercentage()
    {
        var invoice = new Invoice { Id = 1, Amount = 100, TaxRate = 0.21 };

        var result = invoice.Stringify(o => o
            .NoLabel()
            .Only(x => x.TaxRate)
            .For(x => x.TaxRate).NumberFormat("P0"));

        result.Should().Be("TaxRate: 21 %");
    }

    [Test]
    public void NumberFormat_FormatsIntegers()
    {
        var invoice = new Invoice { Id = 1234, Amount = 100, TaxRate = 0 };

        var result = invoice.Stringify(o => o
            .NoLabel()
            .Only(x => x.Id)
            .For(x => x.Id).NumberFormat("D6"));

        result.Should().Be("Id: 001234");
    }

    // ── Highlight ───────────────────────────────────────────────────────────

    [Test]
    public void Highlight_AppliesFormatterWhenPredicateMatches()
    {
        var invoice = new Invoice { Id = 1, Amount = 150, TaxRate = 0 };

        var result = invoice.Stringify(o => o
            .NoLabel()
            .Only(x => x.Amount)
            .For(x => x.Amount).Highlight(v => v > 100, v => $"⚠️ {v:F0}"));

        result.Should().Be("Amount: ⚠️ 150");
    }

    [Test]
    public void Highlight_FallsBackToNormal_WhenPredicateDoesNotMatch()
    {
        var invoice = new Invoice { Id = 1, Amount = 50, TaxRate = 0 };

        var result = invoice.Stringify(o => o
            .NoLabel()
            .Only(x => x.Amount)
            .For(x => x.Amount).Highlight(v => v > 100, v => $"⚠️ {v:F0}"));

        result.Should().Contain("Amount: 50.00");
    }

    // ── Combinations ────────────────────────────────────────────────────────

    [Test]
    public void Transform_CombinesWithTruncate()
    {
        var animal = new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5, Age = 5, IsRare = false };

        var result = animal.Stringify(o => o
            .NoLabel()
            .Only(x => x.Name)
            .For(x => x.Name).UpperCase().Truncate(4));

        result.Should().Be("Name: MITT…");
    }

    [Test]
    public void ReplaceValues_CombinesWithPrefix()
    {
        var task = new Task { Title = "Fix bug", Source = "inbox", Priority = 1 };

        var result = task.Stringify(o => o
            .NoLabel()
            .Only(x => x.Source)
            .For(x => x.Source).ReplaceValues(new() { { "inbox", "Inbox" } }).Prefix("[").Suffix("]"));

        result.Should().Be("Source: [Inbox]");
    }
}
