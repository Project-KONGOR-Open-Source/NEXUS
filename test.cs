using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using PhpSerializerNET;

class Program {
    static void Main() {
        var dict = new OrderedDictionary();
        dict.Add("account_id", "123");
        dict.Add("mvp", "74");
        dict.Add("awd_mann", "21");
        dict.Add("vested_threshold", 5);
        dict.Add(0, 0);
        dict.Add(1, true);
        Console.WriteLine(PhpSerialization.Serialize(dict));
        
        var dict2 = new Dictionary<object, object>();
        dict2.Add("account_id", "123");
        dict2.Add("mvp", "74");
        dict2.Add("awd_mann", "21");
        dict2.Add("vested_threshold", 5);
        dict2.Add(0, 0);
        dict2.Add(1, true);
        Console.WriteLine(PhpSerialization.Serialize(dict2));
    }
}
