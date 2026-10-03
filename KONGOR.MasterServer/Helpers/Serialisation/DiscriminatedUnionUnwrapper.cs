namespace KONGOR.MasterServer.Helpers.Serialisation;

/// <summary>
///     Helper class for unwrapping discriminated union types in PHP serialization.
/// </summary>
/// <remarks>
///     Provides shared logic for recursively unwrapping discriminated union types in nested structures.
///     Used by <see cref="PHPPropertyAttribute"/>.
/// </remarks>
internal static class DiscriminatedUnionUnwrapper
{
    /// <summary>
    ///     Processes a value by recursively unwrapping discriminated union types in nested structures.
    /// </summary>
    public static object? ProcessValue(object? value)
    {
        if (value is null)
            return null;

        // Unwrap The Discriminated Union And Process Its Inner Value, In Case It Is Itself A Discriminated Union Or A Dictionary With Discriminated Union Values
        if (value is IUnion discriminatedUnion)
            return ProcessValue(discriminatedUnion.Value);

        Type valueType = value.GetType();

        // Check If The Value Is A Dictionary With Discriminated Union Values
        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            Type[] dictionaryGenericArguments = valueType.GetGenericArguments();

            Type dictionaryKeyType = dictionaryGenericArguments[0];
            Type dictionaryValueType = dictionaryGenericArguments[1];

            // Check If Dictionary Values Might Contain Discriminated Union Types (Either Directly Or In Nested Structures)
            if (ContainsDiscriminatedUnionTypes(dictionaryValueType))
            {
                Type newDictionaryType = typeof(Dictionary<,>).MakeGenericType(dictionaryKeyType, typeof(object));

                object? newDictionaryInstance = Activator.CreateInstance(newDictionaryType);

                if (newDictionaryInstance is not IDictionary newDictionary)
                    return value;

                foreach (DictionaryEntry entry in (IDictionary) value)
                {
                    object? processedEntry = ProcessValue(entry.Value);

                    newDictionary.Add(entry.Key, processedEntry);
                }

                return newDictionary;
            }
        }

        return value;
    }

    /// <summary>
    ///     Checks whether a type contains discriminated union types, either directly or within nested structures like dictionaries or collections.
    /// </summary>
    private static bool ContainsDiscriminatedUnionTypes(Type type)
    {
        // Check If The Type Itself Is A Discriminated Union
        if (typeof(IUnion).IsAssignableFrom(type))
            return true;

        // Check If The Type Is A Dictionary That Might Contain Discriminated Union Values
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            Type dictionaryValueType = type.GetGenericArguments()[1];

            return ContainsDiscriminatedUnionTypes(dictionaryValueType);
        }

        // Check If The Type Is A Collection That Might Contain Discriminated Union Values
        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        {
            Type collectionElementType = type.GetGenericArguments()[0];

            return ContainsDiscriminatedUnionTypes(collectionElementType);
        }

        return false;
    }
}
