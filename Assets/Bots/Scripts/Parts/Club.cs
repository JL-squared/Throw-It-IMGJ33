using UnityEngine;

public class Club : BotBehaviour {
    public float rpm;
    public Transform rotatingPart;
    public TriggerDamage hurtyPart;

    // Update is called once per frame
    void Update() {
        rotatingPart.Rotate(new Vector3(rpm * 360f * Time.deltaTime * (1 - deathFactor), 0, 0), Space.Self);

        if (deathFactor > 0) {
            hurtyPart.damage = 0;
        }
    }
}
