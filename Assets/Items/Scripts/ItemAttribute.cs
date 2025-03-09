using Newtonsoft.Json;
using System;
using UnityEngine;

public class ItemAttribute : Attribute {
    string name;
    
    public ItemAttribute(string name) {
        this.name = name;
    }
}