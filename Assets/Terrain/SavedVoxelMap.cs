using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// Scriptable object containing a reference to each of the map region raw byte files
// Each 16 chunks is part of a specific file.
// We have to do this so that Git doesn't scream at us telling us we don't have LFS
[CreateAssetMenu(fileName = "Voxel Map", menuName = "Voxel Terrain")]
public class SavedVoxelMap : ScriptableObject {
    public const int ChunksInRegion = 16;
    
    public TextAsset[] textAssets; 
    public Vector3Int mapSize;
}