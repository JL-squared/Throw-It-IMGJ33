using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RngList<T> {
    public List<T> list;
    public bool useLastAsFallback;
}
