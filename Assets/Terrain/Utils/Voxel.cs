using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

// CPU representation of what a voxel is. The most important value here is the density value
[StructLayout(LayoutKind.Sequential)]
public struct Voxel {
    public const int size = sizeof(int);

    // Density of the voxel as a half to save some memory
    public half density;
    
    // Material of the voxel that depicts its color and other parameters
    public byte material;

    // Empty voxel with the empty material
    public readonly static Voxel Empty = new Voxel {
        density = half.zero,
        material = byte.MaxValue,
    };

    public override string ToString() {
        return $"D: {(float)density}, M: {material}";
    }

    // Check if the voxel is a "solid" voxel with a specific material
    public bool IsSolidOfType(byte material) {
        return this.material == material && density < -0.25;
    }
}