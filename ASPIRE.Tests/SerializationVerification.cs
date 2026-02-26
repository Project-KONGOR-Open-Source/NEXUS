using System.Collections;

namespace ASPIRE.Tests;

public class SerializationVerificationTests
{
    [Test]
    public async Task TestSerializationFormats()
    {
        Console.WriteLine("TestSerializationFormats: STARTED");
        Type t = typeof(PhpSerialization);
        Console.WriteLine($"PhpSerialization Assembly: {t.Assembly.FullName}");
        Console.WriteLine($"PhpSerialization Location: {t.Assembly.Location}");

        // Case 1: List<object> (Vector)
        List<object> list = ["Val1", "Val2"];
        string serializedList = PhpSerialization.Serialize(list);
        Console.WriteLine($"List<object>: {serializedList}");

        // Case 2: Dictionary 0-based
        Dictionary<object, object> dict0 = new Dictionary<object, object> { { 0, "Val1" }, { 1, "Val2" } };
        string serializedDict0 = PhpSerialization.Serialize(dict0);
        Console.WriteLine($"Dict 0-based: {serializedDict0}");

        // Case 3: Dictionary 1-based
        Dictionary<object, object> dict1 = new Dictionary<object, object> { { 1, "Val1" }, { 2, "Val2" } };
        string serializedDict1 = PhpSerialization.Serialize(dict1);
        Console.WriteLine($"Dict 1-based: {serializedDict1}");
        
        await Task.CompletedTask;
    }

    [Test]
    public async Task TestDictionarySerialization()
    {
        Console.WriteLine("TestDictionarySerialization: STARTED");
        Dictionary<int, object> dict = new System.Collections.Generic.Dictionary<int, object>
        {
            { 1, "Hero1" },
            { 2, "Hero2" }
        };
        object dictAsObj = dict; // Mimic SimpleStatsHandler
        string output = PhpSerialization.Serialize(dictAsObj);
        Console.WriteLine($"Serialized Dictionary<int, object> (as object): {output}");
        
        List<object> list = new System.Collections.Generic.List<object> { "Item1", "Item2" };
        string listOutput = PhpSerialization.Serialize(list);
        Console.WriteLine($"Serialized List: {listOutput}");

        Type t = typeof(PhpSerialization);
        Console.WriteLine($"PhpSerialization Assembly: {t.Assembly.FullName}");
        await Task.CompletedTask;
    }
    [Test]
    public async Task TestMasterySerialization()
    {
        Console.WriteLine("TestMasterySerialization: STARTED");
        // Replicate ClientRequestHelper structure (Now using Hashtable)
        Hashtable masteryData = new System.Collections.Hashtable();
        
        // Mock data for Hero 139 (from logs)
        int index = 1;
        // FIX: Use Hashtable with INT keys to match ClientRequestHelper fix
        Hashtable heroData = new System.Collections.Hashtable();
        heroData[1] = "HeroIdent";
        heroData[2] = "HeroIdent";
        heroData["heroName"] = "HeroIdent";
        heroData[17] = "1000"; // XP as string
        heroData["level"] = 1;
        
        masteryData[index] = heroData;

        // Cast to object to match SimpleStatsHandler
        object dictAsObj = masteryData; 
        
        string output = PhpSerialization.Serialize(dictAsObj);
        Console.WriteLine($"Serialized Mastery Data (Hashtable/IntKeys): {output}");
        
        await Task.CompletedTask;
    }
}
