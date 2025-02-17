using UnityEngine;

public class Club : BotBehaviour {
    public float baseDamage = 1;
    private float damage;
    public float rpm;
    public Transform rotatingPart;
    public OnTriggerRerouter rerouter;

    private void Start() {
        damage = baseDamage;
        rerouter.onTriggerEnter.AddListener((Collider other) => {
            EntityHealth health = other.gameObject.GetComponent<EntityHealth>();
            if (health != null) {
                health.Damage(damage, new EntityHealth.DamageSourceData() {
                    source = gameObject,
                    direction = (transform.position - other.gameObject.transform.position).normalized,
                });
            }
        });
    }

    // Update is called once per frame
    void Update() {
        rotatingPart.Rotate(new Vector3(rpm * 360f * Time.deltaTime * (1 - deathFactor), 0, 0), Space.Self);

        if (deathFactor > 0) {
            damage = 0;
        }
    }
}
