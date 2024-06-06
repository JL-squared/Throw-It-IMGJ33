using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// might be a new mexico hit drug
public class CrystalOminousRotation : MonoBehaviour {
    public float rotationSpeed;
    public float bobbingSpeed;
    public float bobbingHeight;
    public GameObject mesh;

    void Update() {
        float y = Mathf.Sin(bobbingSpeed * Time.time) * bobbingHeight;
        mesh.transform.localPosition = Vector3.up * y;
        mesh.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
