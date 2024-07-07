using System;
using UnityEngine.Rendering;
using Newtonsoft.Json;
using UnityEngine.Rendering.Universal;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections.Generic;

[Serializable]
public class VoxelTerrainSettings {
    public int jobsPerFrame = 7;
    public int schedulingInnerloopBatchCount = 512;
    public int maxFramesForDeferredEdits = 5;
    public bool neverDeferEdits = false;
}
