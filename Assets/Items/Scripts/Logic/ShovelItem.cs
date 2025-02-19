using Tweens;
using UnityEngine;
using UnityEngine.InputSystem;
using jedjoud.VoxelTerrain;
using jedjoud.VoxelTerrain.Edits;

public class ShovelItem : ToolItem {
    bool canPickupSnow;
    float durability;


    public override void SecondaryAction(InputAction.CallbackContext context, Player player) {
        base.SecondaryAction(context, player);
    }

    public override void PrimaryAction(InputAction.CallbackContext context, Player player) {
        base.PrimaryAction(context, player);

        var shovelItemData = ((ShovelItemData)ItemReference.Data);

        if (!context.canceled && player.lookingAt.HasValue) {
            Vector3 point = player.lookingAt.Value.point;
            Vector3 normal = player.lookingAt.Value.normal;
            GameObject go = player.lookingAt.Value.collider.gameObject;

            if (go.GetComponent<Rigidbody>() is Rigidbody rb && rb != null) {
                Vector3 forward = player.camera.transform.forward;
                rb.AddForceAtPosition(forward * 500.0f, point);
            }

            if (go.GetComponent<EntityHealth>() is EntityHealth health && health != null) {
                health.Damage(5.0f, new EntityHealth.DamageSourceData {
                    source = player.gameObject,
                    direction = player.camera.transform.forward,
                });
            }

            player.inventory.viewModel.CancelTweens();
            
            player.inventory.viewModel.AddTween(new LocalRotationTween {
                from = shovelItemData.viewModelRotationOffset,
                to = shovelItemData.animationRotation,
                duration = 0.2f,
                easeType = EaseType.ElasticOut,
                onEnd = (TweenInstance<Transform, Quaternion> instance) => {


                    player.inventory.viewModel.AddTween(new LocalRotationTween {
                        from = shovelItemData.animationRotation,
                        to = shovelItemData.viewModelRotationOffset,
                        duration = 0.2f,
                        easeType = EaseType.Linear,
                    });
                },
            });

            var gm = GameObject.Instantiate(shovelItemData.particles, point, Quaternion.LookRotation(normal));
            GameObject.Destroy(gm, 0.2f);

            Utils.PlaySound(point, Registries.snowBrickPlace);

            if (canPickupSnow) {
                ShovelItemData data = (ShovelItemData)ItemReference.Data;

                if (VoxelTerrain.Instance != null) {
                    VoxelTerrain.Instance.edits.ApplyVoxelEdit(new AddVoxelEdit {
                        center = point,
                        maskMaterial = true,
                        material = 0,
                        radius = data.digRadius,
                        strength = data.digStrength,
                        writeMaterial = false,
                        scale = Vector3.one,
                    }, true);
                }

                player.inventory.container.PutItem(new ItemStack("snowball", 1));
            }
        }
    }

    public override void Update(Player player) {
        base.Update(player);
    }

    public override void Unequipped(Player player) {
        base.Unequipped(player);
        canPickupSnow = false;
        UIScriptMaster.Instance.crosshairHints.SetRightClickHint(false);
    }

    public override void EquippedUpdate(Player player) {
        canPickupSnow = false;
        if (player.lookingAt.HasValue && player.lookingAt.Value.collider != null) {
            RaycastHit info = player.lookingAt.Value;
            VoxelChunk chunk = info.collider.GetComponent<VoxelChunk>();

            // chunk.GetTriangleIndexMaterialType(info.triangleIndex) == 0 
            if (chunk != null) {
                canPickupSnow = true && player.inventory.container.CanFitItem(new ItemStack("snowball", 1));
            }
        }

        UIScriptMaster.Instance.crosshairHints.SetRightClickHint(canPickupSnow);
    }
}
