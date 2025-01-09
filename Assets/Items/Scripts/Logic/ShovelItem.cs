using Tweens;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShovelItem : ToolItem {
    bool canPickupSnow;
    float durability;

    public override void SecondaryAction(InputAction.CallbackContext context, Player player) {
        base.SecondaryAction(context, player);

        if (canPickupSnow && !context.canceled) {
            ShovelItemData data = (ShovelItemData)player.EquippedItem.Data;
            
            /*
            if (VoxelTerrain.Instance != null) {
                VoxelTerrain.Instance.ApplyVoxelEdit(new AddVoxelEdit {
                    center = player.lookingAt.Value.point,
                    maskMaterial = true,
                    material = 0,
                    radius = data.digRadius,
                    strength = data.digStrength,
                    writeMaterial = false,
                    scale = new float3(1f),
                });
            }
            */

            player.AddItem(new ItemStack("snowball", 1));
        }
    }

    public override void PrimaryAction(InputAction.CallbackContext context, Player player) {
        base.PrimaryAction(context, player);
        var shovelItemData = ((ShovelItemData)ItemReference.Data);
        if (!context.canceled && player.lookingAt.HasValue) {
            bool doTheAnimation = false;


            Rigidbody rb = player.lookingAt.Value.rigidbody;
            if (rb != null) {
                Vector3 forward = player.gameCamera.transform.forward;
                rb.AddForceAtPosition(forward * 500.0f, player.lookingAt.Value.point);
                doTheAnimation = true;
            }

            EntityHealth health = player.lookingAt.Value.collider.gameObject.GetComponent<EntityHealth>();
            if (health != null) {
                doTheAnimation = true;
                health.Damage(5.0f);
            }

            if (doTheAnimation) {
                player.viewModel.AddTween(new LocalRotationTween {
                    from = shovelItemData.viewModelRotationOffset,
                    to = shovelItemData.animationRotation,
                    duration = 0.2f,
                    easeType = EaseType.QuadIn,
                    usePingPong = true,
                });

                player.viewModel.AddTween(new LocalPositionTween {
                    from = shovelItemData.viewModelPositionOffset,
                    to = shovelItemData.animationPosition,
                    duration = 0.2f,
                    easeType = EaseType.QuadIn,
                    usePingPong = true,
                });
            }
        }
    }

    public override void Update(Player player) {
        base.Update(player);
    }

    public override void Unequipped(Player player) {
        base.Unequipped(player);
        canPickupSnow = false;
        UIMaster.Instance.SetRightClickHint(false);
    }

    public override void EquippedUpdate(Player player) {
        canPickupSnow = false;
        if (player.lookingAt != null) {
            RaycastHit info = player.lookingAt.Value;
            /*
            VoxelChunk chunk = info.collider.GetComponent<VoxelChunk>();
            if (chunk != null) {
                canPickupSnow = chunk.GetTriangleIndexMaterialType(info.triangleIndex) == 0 && player.CanFitItem(new ItemStack("snowball", 1));
            }
            */
        }

        UIMaster.Instance.SetRightClickHint(canPickupSnow);
    }
}
