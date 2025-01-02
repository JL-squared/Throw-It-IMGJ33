using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ModelOffset", menuName = "ScriptableObjects/New Model Offset", order = 2)]
public class ModelOffset : ScriptableObject {
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.one;
    public Quaternion rotationOffset = Quaternion.identity;

    public ModelOffset() { }
}