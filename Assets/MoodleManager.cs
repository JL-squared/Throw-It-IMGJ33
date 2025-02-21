using System;
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

    public int spacing;

    public EaseType type;
    public float duration;

    public EaseType shakeType;
    public float shakeDuration;
    public int timesShaken;

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

    private enum ComparisonType {
        Equal,
        GreaterThanOrEqual,
        LesserThanOrEqual,
    }

    PlayerTemperature temperature;
    WeatherManager weatherManager;

    GameObject moodleParent;

    List<Moodle> activeMoodles = new List<Moodle>();

    int timer = 0;

    private void Start() {
        temperature = Player.Instance.temperature;
        weatherManager = GameManager.Instance.weatherManager;
        moodleParent = UIScriptMaster.Instance.inGameHUD.moodleParent;
    }

    private void FixedUpdate() {
        if (timer > 2) {
            Thresholdify(temperature.bodyTemp, minNeutralHypo, minWeakHypo, minMedHypo, minBadHypo, hypothermia);
            Thresholdify(Player.Instance.temperature.sourcesTemp, minNeutralCold, minWeakCold, minMedCold, minBadCold, cold);
            Thresholdify(weatherManager.windSpeed, minNeutralWind, minWeakWind, minMedWind, minBadWind, wind, ComparisonType.GreaterThanOrEqual);
        } else {
            timer++;
        }
    }

    public void Moodlify(MoodleDefinition definition, MoodleStrength strength) {
        bool foundDestroyMoodle = false; // Have we found something to delete (in the case of None)
        int destroyMoodleIndex = 0;

        if (activeMoodles.Count <= 0 && strength == MoodleStrength.None) {
            return;
        }

        int index = 0;
        foreach (Moodle activeMoodle in activeMoodles) {
            if (foundDestroyMoodle) {
                Debug.Log($"Adding a downward tween to moodle {activeMoodle.definition.id}; moodle {activeMoodles[destroyMoodleIndex].definition.id} is to be destroyed");

                var toGo = index * spacing - spacing;
                var tween = new FloatTween {
                    from = activeMoodle.transform.localPosition.y,
                    to = toGo,
                    duration = duration,
                    onUpdate = (_, value) => {
                        activeMoodle.transform.localPosition = new Vector3(activeMoodle.gameObject.transform.localPosition.x, value, activeMoodle.gameObject.transform.localPosition.z);
                    },
                    easeType = type,
                    dontInvokeWhenDestroyed = true
                };

                activeMoodle.gameObject.AddTween(tween);
            } else if (activeMoodle.definition.id == definition.id) {
                // Debug.Log($"Found a matching moodle: {activeMoodle.definition.id}");
                if (strength == MoodleStrength.None) {
                    foundDestroyMoodle = true;
                } else if (strength != activeMoodle.strength) {
                    Debug.Log($"Adding shake tween to {activeMoodle.definition.id}");
                    activeMoodle.Initialize(definition, strength); // Just replace the existing one with a different intensity
                    var tween = new FloatTween {
                        from = 0,
                        to = timesShaken,
                        duration = shakeDuration,
                        onUpdate = (instance, value) => {
                            if (!activeMoodle.IsNullOrDestroyed() && !activeMoodle.gameObject.IsNullOrDestroyed())
                                activeMoodle.transform.localPosition = new Vector3(Mathf.Sin(value * Mathf.PI * 2) * 6, activeMoodle.gameObject.transform.localPosition.y, activeMoodle.gameObject.transform.localPosition.z);
                        },
                        easeType = EaseType.Linear,
                        fillMode = FillMode.None,
                        onCancel = (instance) => {
                            if (!activeMoodle.IsNullOrDestroyed() && !activeMoodle.gameObject.IsNullOrDestroyed())
                                activeMoodle.transform.localPosition = new Vector3(0, activeMoodle.transform.localPosition.y, activeMoodle.transform.localPosition.z);
                        },
                        dontInvokeWhenDestroyed = true
                    };

                    activeMoodle.gameObject.AddTween(tween);
                    return;
                } else {
                    return;
                }
            } else {
                destroyMoodleIndex++;
            }
            index++;
        }

        if (foundDestroyMoodle) {
            var destroyMoodle = activeMoodles[destroyMoodleIndex];
            Debug.Log("Cancelling tweens on " + destroyMoodle.definition.id);
            gameObject.CancelTweens(destroyMoodle.gameObject);
            activeMoodles.RemoveAt(destroyMoodleIndex);
            Destroy(destroyMoodle.gameObject);
            return;
        }

        CreateMoodle(definition, strength);
    }

    public void CreateMoodle(MoodleDefinition definition, MoodleStrength strength) {
        if (strength == MoodleStrength.None) // Since we have to go through the rest of the list make sure to return here
            return;

        Debug.Log($"Creating new moodle {definition.id}");

        var gameObject = Instantiate(UIScriptMaster.Instance.moodlePrefab, moodleParent.transform); // Otherwise make a new moodle!!!!!!!!!
        gameObject.transform.localPosition = new Vector3(0, -spacing, 0);
        var moodle = gameObject.GetComponent<Moodle>();
        moodle.Initialize(definition, strength);
        activeMoodles.Insert(0, moodle);

        int index = 0;
        foreach (Moodle activeMoodle in activeMoodles) {
            Debug.Log($"Adding an upward tween to moodle {activeMoodle.definition.id}; moodle {moodle.definition.id} has been added");

            var toGo = index * spacing; // THIS IS WRONG!!!!
            var tween = new FloatTween {
                from = activeMoodle.transform.localPosition.y,
                to = toGo,
                duration = duration,
                dontInvokeWhenDestroyed = true,
                onUpdate = (_, value) => {
                    if (!activeMoodle.IsNullOrDestroyed() && !activeMoodle.gameObject.IsNullOrDestroyed())
                        activeMoodle.gameObject.transform.localPosition = new Vector3(activeMoodle.gameObject.transform.localPosition.x, value, activeMoodle.gameObject.transform.localPosition.z);
                },
                easeType = type,
            };

            activeMoodle.gameObject.AddTween(tween);
            index++;
        }
    }

    private void Thresholdify<T>(T value, T neutral, T weak, T medium, T bad, MoodleDefinition moodle, ComparisonType comparisonType = ComparisonType.LesserThanOrEqual) where T : IComparable<T> {
        switch (comparisonType) {
            case (ComparisonType.LesserThanOrEqual):
                if (value.CompareTo(bad) <= 0) {
                    Moodlify(moodle, MoodleStrength.Bad);
                }
                else if (value.CompareTo(medium) <= 0) {
                    Moodlify(moodle, MoodleStrength.Medium);
                }
                else if (value.CompareTo(weak) <= 0) {
                    Moodlify(moodle, MoodleStrength.Weak);
                }
                else if (value.CompareTo(neutral) <= 0) {
                    Moodlify(moodle, MoodleStrength.Neutral);
                }
                else {
                    Moodlify(moodle, MoodleStrength.None);
                }
                break;

            case (ComparisonType.GreaterThanOrEqual):
                if (value.CompareTo(bad) >= 0) {
                    Moodlify(moodle, MoodleStrength.Bad);
                }
                else if (value.CompareTo(medium) >= 0) {
                    Moodlify(moodle, MoodleStrength.Medium);
                }
                else if (value.CompareTo(weak) >= 0) {
                    Moodlify(moodle, MoodleStrength.Weak);
                }
                else if (value.CompareTo(neutral) >= 0) {
                    Moodlify(moodle, MoodleStrength.Neutral);
                }
                else {
                    Moodlify(moodle, MoodleStrength.None);
                }
                break;
        }
    }
}
