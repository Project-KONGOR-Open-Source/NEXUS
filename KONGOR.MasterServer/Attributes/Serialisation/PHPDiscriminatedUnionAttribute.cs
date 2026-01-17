namespace KONGOR.MasterServer.Attributes.Serialisation;

/// <summary>
///     PHP serialisation filter for discriminated union properties.
///     This attribute needs to annotate discriminated union properties in order to automatically unwrap and serialise the inner value.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class PHPDiscriminatedUnionAttribute : PhpSerializationFilter
{
    /// <summary>
    ///     Serialises a discriminated union property by unwrapping and serialising the inner value.
    /// </summary>
    /// <remarks>
    ///     This method is invoked by the serialization engine when processing properties annotated with this attribute.
    ///     It is not called directly by application code, but by the library during <see cref="PhpSerialization.Serialize"/> operations.
    /// </remarks>
    public override string? Serialize(object key, object? value, PhpSerializiationOptions options)
    {
        if (value is null)
            return PhpSerialization.Serialize(key, options) + PhpSerialization.Serialize(null, options);

        object? processedValue = DiscriminatedUnionUnwrapper.ProcessValue(value);

        // Concatenate Serialized Key And Serialized Value Without Separator
        // EXAMPLE: key "name" → s:4:"name"; | value "GOPO" → s:4:"GOPO"; | result → s:4:"name";s:4:"GOPO";
        return PhpSerialization.Serialize(key, options) + PhpSerialization.Serialize(processedValue, options);
    }
}
