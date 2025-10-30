using UnityEngine;

public class PlayerMove_ch3 : MonoBehaviour
{
    public float maxSpeed = 5f;
    public bool movable = true;

    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float h = 0f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        animator.SetBool("isWalking", false);
    }

    void Update()
    {
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {

        if (movable)
        {
            h = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            h = 0;
        }

        rigid.linearVelocity = new Vector2(h * maxSpeed, rigid.linearVelocity.y);

        if (h > 0)
            spriteRenderer.flipX = true;
        else if (h < 0)
            spriteRenderer.flipX = false;
            
        animator.SetBool("isWalking", h != 0.0f);
    }

}