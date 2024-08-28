using System;
using UnityEngine.Rendering;
using Newtonsoft.Json;
using UnityEngine.Rendering.Universal;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections.Generic;

[Serializable]
public class PlayerControlsSettings {
    public float mouseSensivity = 1f;
    public float fov = 90f;
    public bool cameraBobbing = true;
    public bool viewModelSway = true;

    // TODO: Add unity new input system control rebinds here
}
