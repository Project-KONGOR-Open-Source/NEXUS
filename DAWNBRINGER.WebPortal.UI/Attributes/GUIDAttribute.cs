namespace DAWNBRINGER.WebPortal.UI.Attributes;

/// <summary>
///     Validates that a property's value is a parseable <see cref="Guid"/> in any of the formats accepted by <see cref="Guid.TryParse(string, out Guid)"/>.
///     <see langword="null"/> and empty values are treated as valid so that <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> remains responsible for enforcing presence.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class GUIDAttribute : ValidationAttribute
{
    public GUIDAttribute() : base("The {0} Field Must Be A Valid GUID") { }

    public override bool IsValid(object? value)
    {
        return value switch
        {
            null        => true,
            string text => string.IsNullOrEmpty(text) || Guid.TryParse(text, out _),
            _           => false
        };
    }
}
