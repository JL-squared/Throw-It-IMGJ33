using UnityEditor;
using UnityEngine;

// Used internally by the classes that handle terrain
public class VoxelBehaviour : MonoBehaviour {
    // Priority of the voxel behaviour for initialization and dispose
    public virtual int Priority { get; } = 0;

    // Fetch the parent terrain heheheha
    [HideInInspector]
    public VoxelTerrain terrain;

    // Initialize the voxel behaviour (called from the voxel terrain)
    public virtual void Init() { }

    // Called after all other voxel behaviours have been initialized
    public virtual void LateInit() { }

    // Dispose of any internally stored memory
    public virtual void Dispose() { }
}