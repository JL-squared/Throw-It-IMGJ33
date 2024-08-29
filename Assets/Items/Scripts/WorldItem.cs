using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WorldItem : MonoBehaviour, IInteraction {
    public ItemStack item;
    public bool Interactable => Player.Instance.CanFitItem(item);
    public bool Highlight => true;
    public GameObject GameObject => gameObject;

    public void Interact(Player player) {
        Destroy(gameObject);
        player.AddItem(item);
    }

    public static bool Spawn(ItemStack item, Vector3 position, Quaternion rotation) {
        if (item.Data.model == null) return false;

        GameObject gameObject = new GameObject();
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        gameObject.name = item.Data.name;

        var worldItemComponent = gameObject.AddComponent<WorldItem>();
        worldItemComponent.item = item;

        gameObject.AddComponent<Rigidbody>();
        var meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        GameObject modelObject;

        modelObject = Instantiate(item.Data.model, gameObject.transform);
        
        Vector3 positionOffset = (item.Data.worldModelOffset == null) ? Vector3.zero : item.Data.worldModelOffset.positionOffset;
        Quaternion rotationOffset = (item.Data.worldModelOffset == null) ? Quaternion.identity : item.Data.worldModelOffset.rotationOffset;
        modelObject.transform.SetLocalPositionAndRotation(positionOffset, rotationOffset);
        meshCollider.sharedMesh = modelObject.GetComponent<MeshFilter>().mesh;
        return true;
    }

    public void Serialize(EntityData data) {
        data.item = item;
    }

    public void Deserialize(EntityData data) {
        item = data.item;
    }
}
