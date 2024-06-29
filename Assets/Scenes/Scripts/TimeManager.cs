using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

// Handles pausing, unpausing, and slowing time when dying
public class TimeManager : MonoBehaviour {
    bool dead;
    float timeSinceDeath;
    public delegate void OnTimeSinceDeathChanged(float timeSinceDeath);
    public event OnTimeSinceDeathChanged onTimeSinceDeath;
    public delegate void OnDeath();
    public event OnDeath onDeath;
    public delegate void OnPauseChanged(bool paused);
    public event OnPauseChanged onPausedChanged;


    private void Update() {
        if (dead) {
            timeSinceDeath += Time.unscaledDeltaTime * .1f;
            Time.timeScale = Mathf.SmoothStep(1.0f, 0.0f, timeSinceDeath);
            onTimeSinceDeath?.Invoke(timeSinceDeath);
        }
    }

    public void PlayedDeadLol() {
        dead = true;
        onDeath?.Invoke();
    }

    public void UpdatePaused(bool paused) {
        Time.timeScale = paused ? 0.0f : 1.0f;
        onPausedChanged?.Invoke(paused);
    }
}
