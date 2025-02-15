using System.Collections.Generic;
using Tweens;
using UnityEngine;

public class MoodleManager : MonoBehaviour {
    public enum MoodleStrength {
        Best,
        Fantastic,
        Great,
        Good,
        None,
        Neutral,
        Weak,
        Medium,
        Bad
    }

    public EaseType type;
    public float duration;

    public Sprite neutralBackground;
    public Sprite weakBackground;
    public Sprite mediumBackground;
    public Sprite badBackground;

    [Header("Windchill")]
    public float minNeutralWind;
    public float minWeakWind;
    public float minMedWind;
    public float minBadWind;
    public MoodleDefinition wind;

    [Header("Cold Environment")]
    public float minNeutralCold;
    public float minWeakCold;
    public float minMedCold;
    public float minBadCold;
    public MoodleDefinition cold;
    
    [Header("Hypothermia")]
    public float minNeutralHypo;
    public float minWeakHypo;
    public float minMedHypo;
    public float minBadHypo;
    public MoodleDefinition hypothermia;

    PlayerTemperature temperature;
    WeatherManager weatherManager;

    GameObject moodleParent;

    List<Moodle> activeMoodles = new List<Moodle>();

    private void Start() {
        temperature = Player.Instance.temperature;
        weatherManager = GameManager.Instance.weatherManager;
        moodleParent = UIScriptMaster.Instance.inGameHUD.moodleParent;
    }

    private void FixedUpdate() {
        if (temperature.bodyTemp <= minBadHypo) {
            Moodlify(hypothermia, MoodleStrength.Bad);
        } else if (temperature.bodyTemp <= minMedHypo) {
            Moodlify(hypothermia, MoodleStrength.Medium);
        } else if (temperature.bodyTemp <= minWeakHypo) {
            Moodlify(hypothermia, MoodleStrength.Weak);
        } else if (temperature.bodyTemp <= minNeutralHypo) {
            Moodlify(hypothermia, MoodleStrength.Neutral);
        } else {
            Moodlify(hypothermia, MoodleStrength.None);
        }
    }

    public void Moodlify(MoodleDefinition definition, MoodleStrength strength) {
        bool thingHasBeenYeeted = false; // Have we found something to delete (in the case of None)

        if (activeMoodles.Count <= 0 && strength == MoodleStrength.None) {
            return;
        }

        foreach (Moodle activeMoodle in activeMoodles) {
            if (thingHasBeenYeeted) {
                var tween = new FloatTween {
                    from = activeMoodle.transform.localPosition.y,
                    to = activeMoodle.transform.localPosition.y - 80,
                    duration = duration,
                    onUpdate = (instance, value) => {
                        activeMoodle.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, value, gameObject.transform.localPosition.z);
                    },
                    easeType = type,
                };

                activeMoodle.gameObject.AddTween(tween);
            } else if (activeMoodle.definition == definition) {
                if (strength == MoodleStrength.None) {
                    Destroy(activeMoodle.gameObject);
                    thingHasBeenYeeted = true;
                } else if (strength != activeMoodle.strength) {
                    activeMoodle.Initialize(definition, strength); // Just replace the existing one with a different intensity
                    return;
                } else {
                    return;
                }
            } 
        }

        if (thingHasBeenYeeted) // Since we have to go through the rest of the list make sure to return here
            return;

        CreateMoodle(definition, strength);
    }

    public void CreateMoodle(MoodleDefinition definition, MoodleStrength strength) {
        var gameObject = Instantiate(UIScriptMaster.Instance.moodlePrefab, moodleParent.transform); // Otherwise make a new moodle!!!!!!!!!
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, -80, gameObject.transform.localPosition.z);
        var moodle = gameObject.GetComponent<Moodle>();
        moodle.Initialize(definition, strength);

        activeMoodles.Insert(0, moodle);

        foreach (Moodle activeMoodle in activeMoodles) {
            var tween = new FloatTween {
                from = activeMoodle.transform.localPosition.y,
                to = activeMoodle.transform.localPosition.y + 80,
                duration = duration,
                onUpdate = (instance, value) => {
                    activeMoodle.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, value, gameObject.transform.localPosition.z);
                },
                easeType = type,
            };

            activeMoodle.gameObject.AddTween(tween);
        }
    }
}
