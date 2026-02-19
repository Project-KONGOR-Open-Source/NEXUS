namespace KONGOR.MasterServer.Helpers.Serialisation;

/// <summary>
///     Provides a type-safe, fluent API for building heterogeneous PHP arrays with mixed key types (string and integer) and mixed value types, without requiring <c>object</c> in the public API.
/// </summary>
/// <remarks>
///     <para>
///         Some game client responses are flat PHP associative arrays with dynamic string keys (e.g. "m0", "m1", ...), fixed string keys (e.g. "vested_threshold"), and integer keys (e.g. 0), all at the same level, with values of different types (objects, integers, booleans).
///         Standard PHP serialisation via <see cref="PhpSerialization.Serialize"/> on a class produces one array entry per property, so the property count must be known at compile time.
///         This does not work when the number of entries is dynamic (e.g. match history with a variable number of "m{N}" entries).
///         Using <c>Dictionary&lt;object, object&gt;</c> would solve the structural problem but sacrifices all type safety.
///     </para>
///     <para>
///         This builder bridges the gap by exposing strongly-typed <see cref="Add"/> overloads while internally collecting entries in an <see cref="OrderedDictionary"/>, which <see cref="PhpSerialization.Serialize"/> handles natively via its <see cref="IDictionary"/> serialisation path.
///         The library resolves each entry's runtime type correctly, including objects annotated with <see cref="PHPPropertyAttribute"/>.
///     </para>
/// </remarks>
public sealed class PHPArrayBuilder
{
    private readonly OrderedDictionary _entries = new ();

    /// <summary>
    ///     Adds a string-keyed entry with a value that has <see cref="PHPPropertyAttribute"/> annotations.
    /// </summary>
    public PHPArrayBuilder Add<TValue>(string key, TValue value) where TValue : class
    {
        _entries.Add(key, value);

        return this;
    }

    /// <summary>
    ///     Adds a string-keyed entry with a string value.
    /// </summary>
    public PHPArrayBuilder Add(string key, string value)
    {
        _entries.Add(key, value);

        return this;
    }

    /// <summary>
    ///     Adds a string-keyed entry with an integer value.
    /// </summary>
    public PHPArrayBuilder Add(string key, int value)
    {
        _entries.Add(key, value);

        return this;
    }

    /// <summary>
    ///     Adds a string-keyed entry with a boolean value.
    /// </summary>
    public PHPArrayBuilder Add(string key, bool value)
    {
        _entries.Add(key, value);

        return this;
    }

    /// <summary>
    ///     Adds an integer-keyed entry with a boolean value.
    /// </summary>
    public PHPArrayBuilder Add(int key, bool value)
    {
        _entries.Add(key, value);

        return this;
    }

    /// <summary>
    ///     Adds an integer-keyed entry with an integer value.
    /// </summary>
    public PHPArrayBuilder Add(int key, int value)
    {
        _entries.Add(key, value);

        return this;
    }

    /// <summary>
    ///     Adds an integer-keyed entry with a string value.
    /// </summary>
    public PHPArrayBuilder Add(int key, string value)
    {
        _entries.Add(key, value);

        return this;
    }

    /// <summary>
    ///     Adds an integer-keyed entry with a value that has <see cref="PHPPropertyAttribute"/> annotations.
    /// </summary>
    public PHPArrayBuilder Add<TValue>(int key, TValue value) where TValue : class
    {
        _entries.Add(key, value);

        return this;
    }

    /// <summary>
    ///     Serialises the collected entries as a PHP associative array.
    /// </summary>
    public string Serialise() => PhpSerialization.Serialize(_entries);
}
