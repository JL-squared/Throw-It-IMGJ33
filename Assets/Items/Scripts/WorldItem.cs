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

    public static GameObject Spawn(ItemStack item, Vector3 position, Quaternion rotation, Vector3 velocity=default) {
        if (item.Data.prefab == null) return null;

        GameObject gameObject = new GameObject();
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.linearVelocity = velocity;
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

        modelObject = Instantiate(item.Data.prefab, gameObject.transform);
        InteractionPropagator propagator = modelObject.AddComponent<InteractionPropagator>();
        propagator.owner = gameObject.GetComponent<IInteraction>();
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
