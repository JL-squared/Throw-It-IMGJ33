using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RngItem {
    public bool enabled = false;
    [Min(0)]
    public float weight = 1f;
}
