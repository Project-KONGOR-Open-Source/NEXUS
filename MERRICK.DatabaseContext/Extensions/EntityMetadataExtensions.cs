namespace MERRICK.DatabaseContext.Extensions;

/// <summary>
///     Extension methods for reading entity metadata that is declared through data annotations.
/// </summary>
public static class EntityMetadataExtensions
{
    /// <summary>
    ///     Returns the maximum length declared by the <see cref="MaxLengthAttribute"/> on the named property of the entity type.
    ///     This lets validation limits track the entity definition directly, rather than being duplicated as constants that can drift out of sync.
    /// </summary>
    public static int GetMaximumLength(this Type entityType, string propertyName)
    {
        MaxLengthAttribute? attribute = entityType.GetProperty(propertyName)?.GetCustomAttribute<MaxLengthAttribute>();

        return attribute?.Length
            ?? throw new InvalidOperationException($@"Property ""{entityType.Name}.{propertyName}"" Does Not Declare A ""MaxLength"" Attribute");
    }
}
