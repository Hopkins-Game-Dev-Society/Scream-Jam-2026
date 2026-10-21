using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController), typeof(PlayerInput))]
public class LootInteraction : MonoBehaviour
{
    [SerializeField] private LootMenu lootMenu;

    [Tooltip("How far (in units) outside a corpse's outline the player's center can be and still loot it.")]
    [SerializeField] private float reach = 1f;

    private PlayerController player;
    private InputAction interact;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        interact = GetComponent<PlayerInput>().actions.FindAction("Player/Interact", true);
    }

    private void Update()
    {
        if (lootMenu.IsOpen)
        {
            return;
        }

        Body corpse = player.IsDead ? null : FindCorpseUnderPlayer();
        lootMenu.ShowPrompt(corpse != null ? $"[{interact.GetBindingDisplayString()}] Loot" : null);

        if (corpse != null && interact.WasPressedThisFrame())
        {
            lootMenu.Open(player, corpse);
        }
    }

    private Body FindCorpseUnderPlayer()
    {
        Vector3 position = transform.position;
        Body nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Body body in Body.All)
        {
            if (body == player.Body || !body.Health.IsDead)
            {
                continue;
            }

            Bounds bounds = body.GetVisualBounds();
            bounds.Expand(reach * 2f);

            if (!bounds.Contains(new Vector3(position.x, position.y, bounds.center.z)))
            {
                continue;
            }

            float distance = ((Vector2)(bounds.center - position)).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = body;
            }
        }

        return nearest;
    }
}
