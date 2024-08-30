using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WorldItem : MonoBehaviour, IInteraction, IEntitySerializer {
    public ItemStack item;
    public bool Interactable => Player.Instance.CanFitItem(item);
    public bool Highlight => true;
    public GameObject GameObject => gameObject;

    public void Interact(Player player) {
        Destroy(gameObject);
        player.AddItem(item);
    }

    public static GameObject Spawn(ItemStack item, Vector3 position, Quaternion rotation) {
        if (item.Data.model == null) return null;

        GameObject gameObject = new GameObject();
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.position = position;
        rb.rotation = rotation;
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        gameObject.name = item.Data.name;

        Entity entity = gameObject.AddComponent<Entity>();
        entity.identifier = "entity:world_item";

        var worldItemComponent = gameObject.AddComponent<WorldItem>();
        worldItemComponent.item = item;

        GameObject modelObject;

        modelObject = Instantiate(item.Data.model, gameObject.transform);
        InteractionPropagator propagator = modelObject.AddComponent<InteractionPropagator>();
        propagator.owner = gameObject.GetComponent<IInteraction>();

        Vector3 positionOffset = (item.Data.worldModelOffset == null) ? Vector3.zero : item.Data.worldModelOffset.positionOffset;
        Quaternion rotationOffset = (item.Data.worldModelOffset == null) ? Quaternion.identity : item.Data.worldModelOffset.rotationOffset;
        Vector3 scaleOffset = (item.Data.worldModelOffset == null) ? Vector3.one : item.Data.worldModelOffset.scaleOffset;
        modelObject.transform.position += positionOffset;
        modelObject.transform.rotation *= rotationOffset;
        modelObject.transform.localScale.Scale(scaleOffset);

        modelObject.transform.SetLocalPositionAndRotation(positionOffset, rotationOffset);
        item.logic.OnWorldItemSpawned(worldItemComponent);
        Utils.ExecuteChildrenRecursive(gameObject.transform, (x) => x.layer = LayerMask.NameToLayer("Interaction"));
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        return gameObject;
    }

    public void Serialize(EntityData data) {
        data.itemStack = item;
    }

    public void Deserialize(EntityData data) {
        item = data.itemStack;
    }
}
