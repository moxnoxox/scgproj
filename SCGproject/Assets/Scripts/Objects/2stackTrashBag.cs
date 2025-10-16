using UnityEngine;

public class TrashBag2Stack : MonoBehaviour
{
    public GameObject topBag;
    public GameObject bottomBag;
    private GameObject player;
    private bool isSeparated = false;
    private int originalOrder;

    void Awake()
    {
        if (topBag.GetComponent<TrashBag1Stack>())
            topBag.GetComponent<TrashBag1Stack>().enabled = false;
        if (bottomBag.GetComponent<TrashBag1Stack>())
            bottomBag.GetComponent<TrashBag1Stack>().enabled = false;

        Rigidbody2D bottomRb = bottomBag.GetComponent<Rigidbody2D>();
        if (bottomRb != null)
        {
            bottomRb.isKinematic = true;
            bottomRb.linearVelocity = Vector2.zero;
            bottomRb.angularVelocity = 0f;
        }

        Rigidbody2D topRb = topBag.GetComponent<Rigidbody2D>();
        if (topRb != null)
        {
            topRb.simulated = false;
            topRb.linearVelocity = Vector2.zero;
            topRb.angularVelocity = 0f;
        }

        Physics2D.SyncTransforms();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isSeparated) return;
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
        if (move == null || move.isHolding || isSeparated) return;

        topBag.transform.parent = null;
        bottomBag.transform.parent = null;

        var top1 = topBag.GetComponent<TrashBag1Stack>();
        if (top1 != null) top1.enabled = true;
        var bottom1 = bottomBag.GetComponent<TrashBag1Stack>();
        if (bottom1 != null) bottom1.enabled = true;

        Rigidbody2D topRb = topBag.GetComponent<Rigidbody2D>();
        if (topRb != null)
        {
            topRb.simulated = true;
            topRb.bodyType = RigidbodyType2D.Dynamic;
            topRb.simulated = false;
        }

        Rigidbody2D bottomRb = bottomBag.GetComponent<Rigidbody2D>();
        if (bottomRb != null)
        {
            bottomRb.isKinematic = false;
            bottomRb.simulated = true;
        }

        move.isHolding = true;
        move.heldObject = topBag;
        topBag.transform.parent = move.transform;

        Animator animator = move.GetComponent<Animator>();
        if (animator != null)
            animator.SetTrigger("hold_start");

        SpriteRenderer sr = move.GetComponent<SpriteRenderer>();
        Vector3 holdPos = move.transform.position + new Vector3(0.1f * (sr.flipX ? 1 : -1), 0.001f, 0);
        topBag.transform.position = holdPos;

        Collider2D[] cols = topBag.GetComponents<Collider2D>();
        foreach (var col in cols)
            col.enabled = false;

        // 💡 플레이어보다 앞으로 보이게 정렬
        var bagRenderer = topBag.GetComponent<SpriteRenderer>();
        var playerRenderer = move.GetComponent<SpriteRenderer>();
        if (bagRenderer != null && playerRenderer != null)
        {
            originalOrder = bagRenderer.sortingOrder;
            bagRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
        }

        move.pickupTarget = null;
        isSeparated = true;
        gameObject.SetActive(false);

        Debug.Log("2단 분리 완료 → topBag 들림 (앞으로 보이게 정렬)");
    }

    public void ResetSortingOrder()
    {
        var sr = topBag.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = originalOrder;
    }
}
