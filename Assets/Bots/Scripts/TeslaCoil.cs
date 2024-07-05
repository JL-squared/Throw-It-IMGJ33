using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeslaCoil : BotBehaviour
{
    public Transform torusTop;
    public float radius = 5f;
    public float healingPerStep = 10f;
    public float secondsBetweenSteps = 1;
    private float nextHealingStep;

    private void Update() {
        if (Time.time > nextHealingStep) {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (var item in colliders) {
                BotBase bot = item.GetComponent<BotBase>();
                if (bot != null && bot != botBase) {
                    bot._bodyHealth.Heal(healingPerStep);
                    bot._headHealth.Heal(healingPerStep);
                }
            }

            nextHealingStep = Time.time + secondsBetweenSteps;
        }
    }
}
