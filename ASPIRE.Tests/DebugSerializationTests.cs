using PhpSerializerNET;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASPIRE.Tests;

public class DebugSerializationTests
{
    [Test]
    public async Task Debug_Serialize_List()
    {
        List<string> list = new List<string> { "a", "b" };
        string serialized = PhpSerialization.Serialize(list);
        Console.WriteLine($"List<string>: {serialized}"); 
        await Task.CompletedTask;
    }

    [Test]
    public async Task Debug_Serialize_Dictionary_IntKey()
    {
        Dictionary<int, object> dict = new Dictionary<int, object> { { 0, "a" }, { 1, "b" } };
        string serialized = PhpSerialization.Serialize(dict);
        Console.WriteLine($"Dictionary<int, object>: {serialized}");
        await Task.CompletedTask;
    }

    [Test]
    public async Task Debug_Serialize_Dictionary_StringKey_Numeric()
    {
        Dictionary<string, object> dict = new Dictionary<string, object> { { "0", "a" }, { "1", "b" } };
        string serialized = PhpSerialization.Serialize(dict);
        Console.WriteLine($"Dictionary<string, object> (Num Keys): {serialized}");
        await Task.CompletedTask;
    }

    [Test]
    public async Task Debug_Serialize_Hybrid()
    {
        Dictionary<object, object> dict = new Dictionary<object, object>();
        dict["nickname"] = "TestUser";
        dict[0] = "val0";
        dict["0"] = "val0"; 
        
        string serialized = PhpSerialization.Serialize(dict);
        Console.WriteLine($"Hybrid Dictionary<object,object>: {serialized}");
        
        if (serialized.Contains("a:0:{}"))
        {
            Assert.Fail("Dictionary<object,object> serialized to empty array!");
        }
        await Task.CompletedTask;
    }

    [Test]
    public async Task Debug_Serialize_Hashtable_IntKeys()
    {
        Hashtable ht = new Hashtable();
        ht[0] = "val0";
        ht[1] = "val1";
        ht["name"] = "test";

        string serialized = PhpSerialization.Serialize(ht);
        Console.WriteLine($"Hashtable (Int/Mixed): {serialized}");
        
        if (serialized.Contains("a:0:{}"))
        {
             Assert.Fail("Hashtable (Int/Mixed) serialized to empty array!");
        }
        await Assert.That(serialized).Contains("val0");
    }
}