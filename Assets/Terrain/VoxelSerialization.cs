using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class VoxelSerialization {
    public static void SerializeIntoRegions<T>(List<T> streamWriters, List<GameObject> totalChunks) where T: Stream {
        byte[] materials = new byte[VoxelUtils.Volume];
        half[] densities = new half[VoxelUtils.Volume];

        for (int r = 0; r < streamWriters.Count; r++) {
            Stream writer = streamWriters[r];

            int offset = SavedVoxelMap.ChunksInRegion * r;

            for (int c = 0; c < SavedVoxelMap.ChunksInRegion; c++) {
                VoxelChunk chunk = totalChunks[c + offset].GetComponent<VoxelChunk>();
                NativeArray<Voxel> voxels = chunk.voxels;

                for (int v = 0; v < voxels.Length; v++) {
                    materials[v] = voxels[v].material;
                }

                //writer.WriteByte(1);
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

    public static void DeserializeFromRegions<T>(List<T> streamReaders, List<GameObject> totalChunks) where T : Stream {
        Span<byte> materials = new Span<byte>(new byte[VoxelUtils.Volume]);
        Span<byte> densities = new Span<byte>(new byte[VoxelUtils.Volume * 2]);

        for (int r = 0; r < streamReaders.Count; r++) {
            Stream reader = streamReaders[r];

            int offset = SavedVoxelMap.ChunksInRegion * r;

            for (int c = 0; c < SavedVoxelMap.ChunksInRegion; c++) {
                reader.Read(materials);

                VoxelChunk chunk = totalChunks[c + offset].GetComponent<VoxelChunk>();
                NativeArray<Voxel> voxels = chunk.voxels;

                for (int v = 0; v < voxels.Length; v++) {
                    voxels[v] = new Voxel() { material = materials[v] };
                }
            }

            for (int c = 0; c < SavedVoxelMap.ChunksInRegion; c++) {
                reader.Read(densities);
                Span<half> halfs = MemoryMarshal.Cast<byte, half>(densities);

                VoxelChunk chunk = totalChunks[c + offset].GetComponent<VoxelChunk>();
                NativeArray<Voxel> voxels = chunk.voxels;

                for (int v = 0; v < voxels.Length; v++) {
                    Voxel temp = voxels[v];
                    //temp.density = half.zero;
                    temp.density = halfs[v];
                    voxels[v] = temp;
                }
            }
        }
    }
}
