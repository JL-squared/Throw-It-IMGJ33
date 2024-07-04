using Unity.Mathematics;
using UnityEngine;

public class ShovelLogic : EquippedItemLogic {
    bool canPickupSnow;
    
    public override void SecondaryAction(bool pressed) {
        base.SecondaryAction(pressed);

        if (canPickupSnow && pressed) {
            ShovelItemData data = (ShovelItemData)equippedItem.Data;
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

            player.AddItem(new Item("snowball", 1));
        }
    }

    public override void Unequipped() {
        base.Unequipped();
        canPickupSnow = false;
        UIMaster.Instance.inGameHUD.SetRightClickHint(false);
    }

    private void Update() {
        canPickupSnow = false;
        if (player.lookingAt != null) {
            RaycastHit info = player.lookingAt.Value;
            VoxelChunk chunk = info.collider.GetComponent<VoxelChunk>();
            if (chunk != null) {
                canPickupSnow = chunk.GetTriangleIndexMaterialType(info.triangleIndex) == 0 && player.CanFitItem(new Item("snowball", 1));
            }
        }

        UIMaster.Instance.inGameHUD.SetRightClickHint(canPickupSnow);
    }
}
