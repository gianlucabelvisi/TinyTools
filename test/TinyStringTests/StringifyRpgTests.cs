using FluentAssertions;
using TinyString;

namespace TinyStringTests;

[TestFixture]
public class StringifyRpgTests
{
    [Stringify(
        PrintStyle = PrintStyle.MultiLine,
        Emoji = "⚔️",
        PrintClassName = true,
        PropertySeparator = "\n",
        CollectionSeparator = ", ",
        Decimals = 1,
        NamingFormat = NamingFormat.HumanCase,
        PropertyFormat = "{k}: {v}",
        ClassNameSeparator = " - "
    )]
    public class Character
    {
        [StringifyProperty(format: "🎭 {v}")]
        public required string Name { get; set; }

        [StringifyProperty(format: "Level {v}")]
        public int Level { get; set; }

        [StringifyProperty(format: "Class: {v}")]
        public CharacterClass Class { get; set; }

        [StringifyProperty(format: "HP: {v}/{v}")]
        public Health Health { get; set; } = new();

        [StringifyProperty(format: "Skills: [{v}]")]
        public List<string> Skills { get; set; } = new();

        [StringifyProperty(format: "💰 {v} gold")]
        public decimal Gold { get; set; }

        [StringifyProperty(format: "🎒 {v}")]
        public Inventory Inventory { get; set; } = new();

        [StringifyIgnore]
        public string SecretPassword { get; set; } = "hidden";

        [StringifyProperty(format: "⭐ {v}")]
        public bool IsLegendary { get; set; }
    }

    [Stringify(
        PrintStyle = PrintStyle.SingleLine,
        PrintClassName = false,
        PropertySeparator = " | ",
        PropertyFormat = "{k}={v}"
    )]
    public class Health
    {
        public int Current { get; set; }
        public int Max { get; set; }
    }

    [Stringify(
        PrintStyle = PrintStyle.MultiLine,
        Emoji = "🎒",
        PrintClassName = false,
        PropertySeparator = "\n  ",
        CollectionSeparator = "\n    ",
        PropertyFormat = "• {k}: {v}"
    )]
    public class Inventory
    {
        public List<Item> Items { get; set; } = new();
        public int Weight { get; set; }
        public int Capacity { get; set; }
    }

    [Stringify(
        PrintStyle = PrintStyle.SingleLine,
        PrintClassName = false,
        PropertySeparator = " ",
        PropertyFormat = "[{k}:{v}]"
    )]
    public class Item
    {
        public string Name { get; set; } = "";
        public ItemType Type { get; set; }
        public int Value { get; set; }
    }

    public enum CharacterClass
    {
        Warrior,
        Mage,
        Rogue,
        Cleric
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Potion,
        Scroll
    }

    [Test]
    public void Character_ShouldStringify_WithAllFeatures()
    {
        var character = new Character
        {
            Name = "Gandalf",
            Level = 99,
            Class = CharacterClass.Mage,
            Health = new Health { Current = 150, Max = 200 },
            Skills = new() { "Fireball", "Teleport", "Lightning Bolt" },
            Gold = 1250.75m,
            Inventory = new Inventory
            {
                Items = new()
                {
                    new Item { Name = "Staff", Type = ItemType.Weapon, Value = 500 },
                    new Item { Name = "Robe", Type = ItemType.Armor, Value = 300 }
                },
                Weight = 15,
                Capacity = 50
            },
            IsLegendary = true
        };

        var result = character.Stringify();

        result.Should().Contain("⚔️ Character");
        result.Should().Contain("🎭 Gandalf");
        result.Should().Contain("Level 99");
        result.Should().Contain("Class: Mage");
        result.Should().Contain("Skills: [Fireball, Teleport, Lightning Bolt]");
        result.Should().Contain("💰 1250.8 gold");
        result.Should().Contain("🎒 🎒");
        result.Should().Contain("• Items:");
        result.Should().Contain("[Name:Staff] [Type:Weapon] [Value:500]");
        result.Should().Contain("[Name:Robe] [Type:Armor] [Value:300]");
        result.Should().Contain("• Weight: 15");
        result.Should().Contain("• Capacity: 50");
        result.Should().Contain("⭐ True");
    }

    [Test]
    public void Character_ShouldIgnore_SecretPassword()
    {
        var character = new Character
        {
            Name = "Secret Agent",
            Level = 1,
            Class = CharacterClass.Rogue,
            Health = new Health { Current = 50, Max = 50 },
            Skills = new() { "Stealth" },
            Gold = 100m,
            Inventory = new Inventory(),
            SecretPassword = "super_secret_123",
            IsLegendary = false
        };

        var result = character.Stringify();

        result.Should().NotContain("Secret Password");
        result.Should().NotContain("super_secret_123");
        result.Should().Contain("🎭 Secret Agent");
    }

    [Test]
    public void Character_ShouldHandle_NullAndEmptyCollections()
    {
        var character = new Character
        {
            Name = "Empty Character",
            Level = 1,
            Class = CharacterClass.Warrior,
            Health = new Health { Current = 100, Max = 100 },
            Skills = null!,
            Gold = 0m,
            Inventory = new Inventory { Items = new() },
            IsLegendary = false
        };

        var result = character.Stringify();

        result.Should().Contain("Skills: [null]");
        result.Should().Contain("🎒 🎒");
        result.Should().Contain("• Items:");
    }
} 