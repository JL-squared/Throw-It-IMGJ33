using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameHUD : MonoBehaviour {
    public GameObject craftingMenuObject;
    public GameObject rightClickHint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRightClickHint(bool on) {
        rightClickHint.SetActive(on);
    }
}
