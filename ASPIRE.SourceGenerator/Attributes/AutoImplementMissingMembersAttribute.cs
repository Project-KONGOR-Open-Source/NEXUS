namespace ASPIRE.SourceGenerator.Attributes;

/// <summary>
///     Marks a partial class for automatic implementation of missing interface members.
///     The source generator will create stub implementations that throw <see cref="NotImplementedException"/> for all interface members not explicitly implemented in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AutoImplementMissingMembersAttribute : Attribute;
