namespace KONGOR.MasterServer.Helpers.Serialisation;

/// <summary>
///     Helper class for unwrapping discriminated union types in PHP serialization.
/// </summary>
/// <remarks>
///     Provides shared logic for recursively unwrapping discriminated union types in nested structures.
///     Used by both <see cref="PHPPropertyAttribute"> and <see cref="PHPDiscriminatedUnionAttribute">.
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

        Type valueType = value.GetType();

        // Check If The Value Itself Is A Discriminated Union Type (Currently, Provided By The "OneOf" Library)
        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition().FullName?.Contains("OneOf") == true)
            return UnwrapDiscriminatedUnionValue(value);

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
        // Check If The Type Itself Is A Discriminated Union (Currently, Provided By The "OneOf" Library)
        if (type.IsGenericType && type.GetGenericTypeDefinition().FullName?.Contains("OneOf") == true)
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

    /// <summary>
    ///     Unwraps a <see cref="OneOf{T1, T2}"/> discriminated union value by calling its <see cref="OneOf{T1, T2}.Match"/> method to extract the inner value.
    /// </summary>
    private static object? UnwrapDiscriminatedUnionValue(object discriminatedUnionValue)
    {
        Type discriminatedUnionType = discriminatedUnionValue.GetType();

        Type[] genericArguments = discriminatedUnionType.GetGenericArguments();

        // Find The Generic "Match" Method, Which Indicates A "OneOf" Discriminated Union Type
        MethodInfo? matchMethod = discriminatedUnionType.GetMethods()
            .SingleOrDefault(method => method.Name == "Match"
                && method.IsGenericMethod && method.GetGenericArguments().Length == 1 && method.GetParameters().Length == genericArguments.Length);

        if (matchMethod is null)
            return discriminatedUnionValue;

        // Make The "Match" Method Concrete With Return Type Object
        MethodInfo concreteMatchMethod = matchMethod.MakeGenericMethod(typeof(object));

        // Get The "Identity" Method For Creating Delegates
        MethodInfo? identityMethod = typeof(DiscriminatedUnionUnwrapper).GetMethod(nameof(Identity), BindingFlags.NonPublic | BindingFlags.Static);

        if (identityMethod is null)
            return discriminatedUnionValue;

        // Create Delegates That Cast Each Possible Type To Object
        object?[] matchDelegates = genericArguments
            .Select(argumentType => Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(argumentType, typeof(object)), identityMethod.MakeGenericMethod(argumentType))).ToArray();

        return concreteMatchMethod.Invoke(discriminatedUnionValue, matchDelegates);
    }

    /// <summary>
    ///     This method is used via reflection to create delegates for the <see cref="OneOf{T1, T2}.Match"/> method.
    ///     Each possible type in the discriminated union needs a <c>Func&lt;T, object&gt;</c> delegate that converts it to object.
    ///     This method simply returns the input value cast to object, preserving <see langword="null"/> values.
    /// </summary>
    /// <remarks>
    ///     EXAMPLE: For <c>OneOf&lt;int, string&gt;</c>, we create <c>Identity&lt;int&gt;</c> and <c>Identity&lt;string&gt;</c> delegates.
    /// </remarks>
    private static object? Identity<T>(T value) => value is null ? null : value;
}
