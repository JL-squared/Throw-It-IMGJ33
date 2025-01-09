using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class VoxelGridSpawner : VoxelBehaviour {
    public Vector3Int mapChunkSize;

    struct TestJob : IJobParallelFor {
        public NativeArray<Voxel> voxels;
        
        public void Execute(int index) {
            float density = (float)VoxelUtils.IndexToPos(index).y - 5f;
            
            voxels[index] = new Voxel() {
                density = (half)density,
                //material = 0,
            };
        }
    }

    public override void LateInit() {
        var executor = GetComponent<VoxelGraphExecutor>();

        for (int x = -mapChunkSize.x; x < mapChunkSize.x; x++) {
            for (int y = -mapChunkSize.y; y < mapChunkSize.y; y++) {
                for (int z = -mapChunkSize.z; z < mapChunkSize.z; z++) {
                    Vector3 position = new Vector3(x, y, z) * VoxelUtils.Size * VoxelUtils.VoxelSizeFactor;
                    var container = new UniqueVoxelContainer();
                    //var temp = (new TestJob { voxels = container.voxels }).Schedule(VoxelUtils.Volume, 2048);
                    //VoxelChunk chunk = terrain.FetchChunk(container, position, 1.0f);
                    //chunk.dependency = temp;
                    executor.ExecuteShader(64, position / VoxelUtils.VertexScaling, Vector3.one * 1.0f, true);
                    
                    AsyncGPUReadback.RequestIntoNativeArray(
                        ref container.voxels,
                        executor.Textures["voxels"], 0,
                        delegate (AsyncGPUReadbackRequest asyncRequest) {
                            VoxelChunk chunk = terrain.FetchChunk(container, position, 1.0f);
                            terrain.GetBehaviour<VoxelMesher>().GenerateMesh(chunk, true);
                        }
                    );
                }
            }
        }
    }
}
