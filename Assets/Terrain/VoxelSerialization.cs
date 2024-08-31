using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class VoxelSerialization {
    public static void SerializeIntoRegions<T>(List<T> streamWriters, List<GameObject> totalChunks, Vector3Int mapSize) where T: Stream {
        byte[] materials = new byte[VoxelUtils.Volume];
        half[] densities = new half[VoxelUtils.Volume];

        for (int r = 0; r < streamWriters.Count; r++) {
            Stream writer = streamWriters[r];

            if (writer == null) {
                continue;
            }

            int offset = SavedVoxelMap.ChunksInRegion * r;

            
            for (int c = 0; c < SavedVoxelMap.ChunksInRegion; c++) {
                VoxelChunk chunk = totalChunks[c + offset].GetComponent<VoxelChunk>();
                NativeArray<Voxel> voxels = chunk.voxels;

                for (int v = 0; v < voxels.Length; v++) {
                    materials[v] = voxels[v].material;
                }

                writer.WriteAsync(materials);
            }
            
            for (int c = 0; c < SavedVoxelMap.ChunksInRegion; c++) {
                VoxelChunk chunk = totalChunks[c + offset].GetComponent<VoxelChunk>();
                NativeArray<Voxel> voxels = chunk.voxels;

                for (int v = 0; v < voxels.Length; v++) {
                    densities[v] = voxels[v].density;
                }

                ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes<half>(densities);
                writer.WriteAsync(bytes.ToArray());
            }
        }
    }

    public static void DeserializeFromRegions<T>(List<T> streamReaders, List<GameObject> totalChunks, Vector3Int mapSize) where T : Stream {
        List<VoxelChunk> chunks = totalChunks.Select((x) => x.GetComponent<VoxelChunk>()).ToList();

        List<Task> actFinal = new List<Task>();
        for (int r = 0; r < streamReaders.Count; r++) {
            Stream reader = streamReaders[r];

            if (reader == null)
                continue;

            int offset = SavedVoxelMap.ChunksInRegion * r;
            Task t = Task.Run(async () => {
                for (int c = 0; c < SavedVoxelMap.ChunksInRegion; c++) {
                    VoxelChunk chunk = chunks[c + offset];
                    Memory<byte> materials = new Memory<byte>(new byte[VoxelUtils.Volume]);
                    Task task = reader.ReadAsync(materials).AsTask().ContinueWith((task) => {

                        NativeArray<Voxel> voxels = chunk.voxels;

                        for (int v = 0; v < voxels.Length; v++) {
                            voxels[v] = new Voxel() { material = materials.Span[v] };
                        }
                    });
                }

                // ReadAsync seems to alr be blocking, no need to do shit? maybe idk

                List<Task> final = new List<Task>();
                for (int c = 0; c < SavedVoxelMap.ChunksInRegion; c++) {
                    Memory<byte> densities = new Memory<byte>(new byte[VoxelUtils.Volume * 2]);
                    VoxelChunk chunk = chunks[c + offset];

                    Task task = reader.ReadAsync(densities).AsTask().ContinueWith((task) => {
                        Span<half> halfs = MemoryMarshal.Cast<byte, half>(densities.Span);

                        NativeArray<Voxel> voxels = chunk.voxels;

                        for (int v = 0; v < voxels.Length; v++) {
                            Voxel temp = voxels[v];
                            temp.density = halfs[v];
                            voxels[v] = temp;
                        }
                    });
                    final.Add(task);
                }

                await Task.WhenAll(final);
            });
            actFinal.Add(t);
        }

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        Task.WhenAll(actFinal).Wait();
        Debug.Log(sw.ElapsedMilliseconds);
    }
}
