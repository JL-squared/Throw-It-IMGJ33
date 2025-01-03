using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;



public class MainMenuUI : MonoBehaviour {
    public TMP_InputField inputField;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="saucington"></param>
    public void Load() {

    }

    /// <summary>
    /// Called from a UI button to create a new save
    /// </summary>
    public void Create() {
        PersistentSaveManager.Instance.Create(inputField.text);
    }
}
