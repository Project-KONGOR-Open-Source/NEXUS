namespace KONGOR.MasterServer.Attributes.Serialisation;

/// <summary>
///     PHP property attribute that combines property name mapping and optional discriminated union handling.
/// </summary>
/// <remarks>
///     When IsDiscriminatedUnion is TRUE, it unwraps discriminated union types before serialisation.
///     When IsDiscriminatedUnion is FALSE, it behaves as a standard property name mapping attribute.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class PHPPropertyAttribute : PhpSerializationFilter
{
    /// <summary>
    ///     The PHP property name or index (preserved as original type for correct serialization).
    /// </summary>
    /// <remarks>
    ///     Stores either an integer index (T0) or a string name (T1).
    ///     The type is preserved to ensure correct PHP serialization format (i:0 vs s:1:"0").
    /// </remarks>
    public OneOf<int, string> PropertyKey { get; }

    /// <summary>
    ///     Whether or not the property annotated with this attribute is a discriminated union type that requires unwrapping before serialization.
    /// </summary>
    public bool IsDiscriminatedUnion { get; }

    /// <summary>
    ///     Initialises a new instance with a string property name.
    /// </summary>
    /// <param name="name">The PHP property name.</param>
    /// <param name="isDiscriminatedUnion">Whether this property is a discriminated union type (defaults to FALSE).</param>
    public PHPPropertyAttribute(string name, bool isDiscriminatedUnion = false)
    {
        PropertyKey = name;
        IsDiscriminatedUnion = isDiscriminatedUnion;
    }

    /// <summary>
    ///     Initialises a new instance with an integer property index.
    /// </summary>
    /// <param name="index">The PHP property index.</param>
    /// <param name="isDiscriminatedUnion">Whether this property is a discriminated union type (defaults to FALSE).</param>
    public PHPPropertyAttribute(int index, bool isDiscriminatedUnion = false)
    {
        PropertyKey = index;
        IsDiscriminatedUnion = isDiscriminatedUnion;
    }

    /// <summary>
    ///     Serialises the property with proper name mapping and optional discriminated union unwrapping.
    /// </summary>
    /// <remarks>
    ///     This method is invoked by the serialization engine when processing properties annotated with this attribute.
    ///     It is not called directly by application code, but by the library during <see cref="PhpSerialization.Serialize"/> operations.
    /// </remarks>
    public override string? Serialize(object key, object? value, PhpSerializiationOptions options)
    {
        // Use The Configured Property Name/Index Instead Of The Default Key
        // Unwrap The Discriminated Union To Get The Actual Integer Or String Value
        object propertyKey = PropertyKey.Match<object>(intKey => intKey, stringKey => stringKey);

        if (value is null)
            return PhpSerialization.Serialize(propertyKey, options) + PhpSerialization.Serialize(null, options);

        // If This Is A Discriminated Union Property, Process It To Unwrap The Inner Value
        object? processedValue = IsDiscriminatedUnion ? DiscriminatedUnionUnwrapper.ProcessValue(value) : value;

        // Concatenate Serialised Key And Serialised Value
        // EXAMPLE: key "name" → s:4:"name"; | value "GOPO" → s:4:"GOPO"; | result → s:4:"name";s:4:"GOPO";
        return PhpSerialization.Serialize(propertyKey, options) + PhpSerialization.Serialize(processedValue, options);
    }
}
