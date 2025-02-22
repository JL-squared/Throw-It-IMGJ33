using UnityEngine;
using UnityEngine.UI;
using static MoodleManager;

public class Moodle : MonoBehaviour {
    public Image background;
    public Image icon;

    public MoodleDefinition definition;
    public MoodleStrength strength;

    public bool toDestroy = false;

    public void Initialize(MoodleDefinition definition, MoodleStrength strength) {
        this.definition = definition;
        this.strength = strength;

        icon.sprite = definition.sprite;
        switch (strength) {
            case MoodleStrength.Neutral:
                background.sprite = Player.Instance.moodleManager.neutralBackground;
                break;

            case MoodleStrength.Weak:
                background.sprite = Player.Instance.moodleManager.weakBackground;
                break;

            case MoodleStrength.Medium:
                background.sprite = Player.Instance.moodleManager.mediumBackground;
                break;

            case MoodleStrength.Bad:
                background.sprite = Player.Instance.moodleManager.badBackground;
                break;
        }
    }
}
