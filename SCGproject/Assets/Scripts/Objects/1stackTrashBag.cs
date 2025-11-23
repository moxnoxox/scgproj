using UnityEngine;

public class TrashBag1Stack : MonoBehaviour, IInteractable
{
    private int originalOrder; 

    // The player now detects this object, so OnTrigger methods are no longer needed here.

    // This is the implementation of the IInteractable interface.
    public void Interact(PlayerMove player)
    {
        // When the player interacts, call the TryLift method.
        if(Chapter2Manager.Instance != null) if (Chapter2Manager.Instance.canHold) TryLift(player);
    }

    public void TryLift(PlayerMove move)
    {
        if (move == null || move.isHolding) return;
        SoundManagerCh2.Instance.PlayPlasticBag();
        move.isHolding = true;
        move.heldObject = gameObject;

        if (move.animator != null)
            move.animator.SetTrigger("hold_start");

        transform.parent = move.transform;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
        }

        Collider2D[] cols = GetComponents<Collider2D>();
        foreach (var col in cols)
            col.enabled = false;

        var bagRenderer = GetComponent<SpriteRenderer>();
        var playerRenderer = move.GetComponent<SpriteRenderer>();
        if (bagRenderer != null && playerRenderer != null)
        {
            originalOrder = bagRenderer.sortingOrder;
            bagRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
        }

        Debug.Log("🧤 1단 쓰레기 들었음 (앞으로 보이게 정렬)");
    }

    public void ResetSortingOrder()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = originalOrder;
    }

    public static void ResetHeldStatus() { }
}
