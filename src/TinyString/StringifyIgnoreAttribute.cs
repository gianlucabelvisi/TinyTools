namespace TinyString;

/// <remarks>
/// Deprecated: use <c>.For(x =&gt; x.Prop).Ignore()</c> in the fluent API instead.
/// This attribute will be removed in a future major version.
/// </remarks>
[Obsolete(
    "Attribute-based configuration is deprecated. " +
    "Use .For(x => x.Prop).Ignore() in the fluent Stringify<T>(Action<StringifyOptions<T>>) API instead. " +
    "This attribute will be removed in a future major version.")]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class StringifyIgnoreAttribute : Attribute
{
}
