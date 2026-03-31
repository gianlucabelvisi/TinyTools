// Required for C# 9+ records and init-only setters when targeting netstandard2.1.
// The type is defined in .NET 5+ but not in netstandard2.1.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
