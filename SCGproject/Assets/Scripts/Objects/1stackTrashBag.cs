using UnityEngine;

public class TrashBag1Stack : MonoBehaviour
{
    private GameObject player;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            var move = player.GetComponent<PlayerMove>();
            if (move != null && !move.isHolding)
                move.pickupTarget = this.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var move = other.GetComponent<PlayerMove>();
            if (move != null && move.pickupTarget == this.gameObject)
                move.pickupTarget = null;
        }
    }

    public void TryLift(PlayerMove move)
    {
        if (move == null || move.isHolding) return;

        move.isHolding = true;
        move.heldObject = gameObject;

        if (move.animator != null)
            move.animator.SetTrigger("hold_start");

        transform.parent = move.transform;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
            rb.velocity = Vector2.zero;
        }

        Collider2D[] cols = GetComponents<Collider2D>();
        foreach (var col in cols)
            col.enabled = false;

        move.pickupTarget = null;

        Debug.Log("🧤 1단 쓰레기 들었음 (PlayerMove 입력으로 Lift)");
    }

    public static void ResetHeldStatus() { }
}
