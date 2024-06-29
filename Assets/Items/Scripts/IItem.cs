using System;
using UnityEngine;

// Base interface for implementing custom item types with custom properties
// All derived objects MUST be structs otherwise we won't be able to serialize them properly
public interface IItem {
    public ItemType ItemType { get; set; }
    public int Amount { get; set; }

    // Must be called whenever the item count is updated
    public delegate void ItemAmountChanged(IItem item);
    public event ItemAmountChanged onItemAmountChanged;

    // Clone this item into another one
    public abstract IItem Clone();

    // Clone this item with a specific count
    public abstract IItem Clone(int count);
}
