using System;
using System.Threading.Tasks;
using TUnit.Core;

namespace ASPIRE.Tests;

public class FindPhpSerializationTests
{
    [Test]
    public async Task TestDictionarySerialization()
    {
        Dictionary<int, object> dict = new System.Collections.Generic.Dictionary<int, object>
        {
            { 1, "Hero1" },
            { 2, "Hero2" }
        };
        string output = PhpSerialization.Serialize(dict);
        Console.WriteLine($"Serialized Dictionary<int, object>: {output}");
        
        List<object> list = new System.Collections.Generic.List<object> { "Item1", "Item2" };
        string listOutput = PhpSerialization.Serialize(list);
        Console.WriteLine($"Serialized List: {listOutput}");

        Type t = typeof(PhpSerialization);
        Console.WriteLine($"PhpSerialization Assembly: {t.Assembly.FullName}");
        await Task.CompletedTask;
    }
}
