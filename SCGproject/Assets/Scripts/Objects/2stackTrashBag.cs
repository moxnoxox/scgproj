using UnityEngine;

public class TrashBag2Stack : MonoBehaviour
{
    public GameObject topBag;
    public GameObject bottomBag;
    private GameObject player;
    private bool isSeparated = false;

    void Awake()
    {
        // 💡 2단 상태일 때 1단 스크립트 비활성화
        if (topBag.GetComponent<TrashBag1Stack>())
            topBag.GetComponent<TrashBag1Stack>().enabled = false;
        if (bottomBag.GetComponent<TrashBag1Stack>())
            bottomBag.GetComponent<TrashBag1Stack>().enabled = false;

        // 💡 하단 봉지는 고정 (밀리지 않게)
        Rigidbody2D bottomRb = bottomBag.GetComponent<Rigidbody2D>();
        if (bottomRb != null)
        {
            bottomRb.isKinematic = true;
            bottomRb.velocity = Vector2.zero;
            bottomRb.angularVelocity = 0f;
        }

        // 💡 상단 봉지는 중력/충돌 비활성화 (떨어지지 않게)
        Rigidbody2D topRb = topBag.GetComponent<Rigidbody2D>();
        if (topRb != null)
        {
            topRb.simulated = false;
            topRb.velocity = Vector2.zero;
            topRb.angularVelocity = 0f;
        }

        // 💡 초기 위치 강제 동기화 (물리 갱신 전에 좌표 고정)
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

        // 💡 2단 분리
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
            topRb.simulated = false; // 들릴 때 중력 비활성화
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
        Vector3 holdPos = move.transform.position + new Vector3(0.8f * (sr.flipX ? 1 : -1), 0.3f, 0);
        topBag.transform.position = holdPos;

        Collider2D[] cols = topBag.GetComponents<Collider2D>();
        foreach (var col in cols)
            col.enabled = false;

        move.pickupTarget = null;
        isSeparated = true;
        gameObject.SetActive(false);

        Debug.Log("2단 분리 완료 → topBag 들림 (Awake 초기화로 튐 방지)");
    }
}
