using UnityEngine;
using UnityEngine.SceneManagement;

public class SomethingSomethingQuitGame : MonoBehaviour {
    public void QuitGame() {
        Application.Quit();
    }

    public void Restart() {
        SceneManager.LoadScene("SampleScene");
    }
}
