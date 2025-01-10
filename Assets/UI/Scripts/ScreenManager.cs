using UnityEngine;

/// <summary>
/// State manager for top-level screen states.
/// This manager informs other classes on controls and movement
/// </summary>
public class ScreenManager : MonoBehaviour {
    // Stuff like mouse,

    public bool MovementAvailable {
        get; private set;
    }
}