namespace TinyString;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
public class RoundingAttribute(int decimals) : Attribute
{
    public int Decimals { get; } = decimals;
}
