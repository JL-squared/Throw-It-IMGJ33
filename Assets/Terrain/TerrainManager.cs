using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles spawning the chunks around the player or at the start of the game
// Nothing to do on the gpu as the terrain map is already pre-generated
// For terrain map editing / creation we will use Burst and the built-in noise class to create custom brushes
public class TerrainManager : MonoBehaviour
{
    public GameObject chunk;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
