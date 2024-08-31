using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;



public class MainMenuUI : MonoBehaviour {
    public TMP_InputField inputField;

    public void Load(string saucington=null) {
        if (saucington == null)
            saucington = inputField.text;

        if (PersistentSaveManager.Instance.LoadFromMenu(inputField.text)) {
            SceneManager.LoadScene("SampleScene");
        }
    }

    public void Create() {
        if (PersistentSaveManager.Instance.Create(inputField.text)) {
            SceneManager.LoadScene("SampleScene");
        }
    }
}
