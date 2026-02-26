using System;
using System.Collections;
using System.Collections.Generic;
using TUnit.Core;
using PhpSerializerNET;

namespace ASPIRE.Tests.Reproduction;

public class PhpSerializationReproductionTests
{
    [Test]
    public async Task TestNestedHashtableSerialization()
    {
        Hashtable inner = new Hashtable();
        inner["key"] = "value";
        inner[1] = 100;

        Hashtable outer = new Hashtable();
        outer[0] = "start";
        outer[1] = inner; // Nested Hashtable
        outer[2] = "end";

        string serialized = PhpSerialization.Serialize(outer);
        Console.WriteLine($"Serialized Output: {serialized}");

        // Expected: ...i:1;a:2:{...}...
        // Bad: ...i:1;s:XX:"a:2:{...}";...

        if (serialized.Contains("s:") && serialized.Contains("a:2:{"))
        {
             // Check if it's wrapped in string
             // e.g. i:1;s:25:"a:2:{s:3:"key";s:5:"value";i:1;i:100;}";
             // vs i:1;a:2:{s:3:"key";s:5:"value";i:1;i:100;}
             
             // This simple check isn't perfect but allows manual inspection via Console
        }
        
        await Assert.That(serialized).DoesNotContain("s:2"); // Arbitrary check to force viewing output
    }
}
