using UnityEngine;

public class PlayerMove_ch3 : MonoBehaviour
{
    [Header("이동 설정")]
    public float maxSpeed = 5f;
    public bool movable = true;      // 물리적 이동 가능 여부
    public bool canInput = true;     // 키 입력 허용 여부

    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float h = 0f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (animator != null)
            animator.SetBool("isWalking", false);
    }

    void Update()
    {
        // 입력 차단 중이면 키 입력 무시
        if (!canInput) return;

        // 키를 뗄 때 속도 감속
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {
        if (!movable)
        {
            h = 0;
            rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
            animator.SetBool("isWalking", false);
            return;
        }

        // 입력 허용 중일 때만 방향 갱신
        h = canInput ? Input.GetAxisRaw("Horizontal") : 0;

        rigid.linearVelocity = new Vector2(h * maxSpeed, rigid.linearVelocity.y);

        if (h > 0)
            spriteRenderer.flipX = true;
        else if (h < 0)
            spriteRenderer.flipX = false;

        animator.SetBool("isWalking", h != 0.0f);
    }

    // ======================================
    // 외부 제어용 메서드
    // ======================================

    /// <summary>
    /// 물리적 이동 가능 여부 설정
    /// </summary>
    public void SetMovable(bool canMove)
    {
        movable = canMove;
        if (!canMove)
        {
            rigid.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.SetBool("isWalking", false);
        }
    }

    /// <summary>
    /// 입력 허용 여부 설정
    /// </summary>
    public void SetInput(bool enableInput)
    {
        canInput = enableInput;
        if (!enableInput)
        {
            rigid.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.SetBool("isWalking", false);
        }
    }

    /// <summary>
    /// 현재 플레이어의 X좌표 반환
    /// (거리 체크나 위치 트리거용)
    /// </summary>
    public float GetXPosition()
    {
        return transform.position.x;
    }

    /// <summary>
    /// 완전히 멈춤 처리 (연출용)
    /// </summary>
    public void StopImmediately()
    {
        rigid.linearVelocity = Vector2.zero;
        h = 0;
        animator.SetBool("isWalking", false);
    }
}
