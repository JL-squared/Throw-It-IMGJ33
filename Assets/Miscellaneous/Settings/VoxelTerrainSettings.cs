using System;

[Serializable]
public class VoxelTerrainSettings {
    public int jobsPerFrame = 7;
    public int schedulingInnerloopBatchCount = 512;
    public int maxFramesForDeferredEdits = 5;
    public bool neverDeferEdits = false;
}
