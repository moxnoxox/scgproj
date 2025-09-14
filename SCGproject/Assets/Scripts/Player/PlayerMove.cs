using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed = 5f;
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    public Animator animator;
    private bool start = false;
    public bool starting = false;
    public bool canInput = true;
    private int sleepcount = 0;
    public player_power playerPower;
    public key_info keyInfo;
    public GameManager gameManager;
    public CanvasGroup canvas;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();
        animator.SetBool("isWalking", false);
        animator.SetBool("isSleep", true); // 시작 시 잠든 상태
        animator.SetBool("isPhone", false);
        keyInfo.isBed = true;
    }

    public void WakeUpExternal()
    {
        if (animator.GetBool("isSleep"))
        {
            animator.SetBool("isSleep", false);
        }
    }
    public void SleepExternal()
    {
        if (!animator.GetBool("isSleep"))
        {
            animator.SetBool("isSleep", true);
        }
    }

    void Update()
    {
        // 좌우 이동 키를 뗄 때 속도 감소
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }
    }

    IEnumerator StartWait()
    {
        yield return new WaitForSeconds(1.0f);
        start = true;
        animator.SetBool("isSleep", false); // 일어나기
        starting = false;
        if (keyInfo != null)
        {
            keyInfo.isBed = false;
        }
    }
    private bool hasMoved = false;
    private Coroutine startIndicatorCoroutine;

    void FixedUpdate()
    {
        if (!canInput) return;
        float h = Input.GetAxisRaw("Horizontal");

        // S키로 시작
        if (!start && !starting && animator.GetBool("isSleep") == false)
        {

            starting = true;
            animator.SetBool("start_trigger", true);
            StartCoroutine(StartWait());
        }

        // 처음으로 h가 0이 아닌 순간 감지
        if (start && h != 0)
        {
            gameManager.OnPlayerMoved();
            hasMoved = true;
            if (startIndicatorCoroutine != null)
                StopCoroutine(startIndicatorCoroutine);
            startIndicatorCoroutine = StartCoroutine(DisableStartIndicator());
        }

        // 파워가 없으면 침대로 이동
        if (GameManager.Instance.autoMove == true && playerPower.noPower == true)
        {
            if (sleepcount == 0)
            {
                StartCoroutine(clickIndicator());
            }
            
            h = -transform.position.x;
            if (h > 0.1f) h = 1;
            else if (h < -0.1f) h = -1;
            else h = 0;
        }
        if(keyInfo.is_click && animator.GetBool("isPhone") == true)
        {
            keyInfo.is_click = false;
        }

        // 잠든 상태면 파워 회복, 이동 불가
        if (animator.GetBool("isSleep"))
        {
            if (playerPower != null)
                if (playerPower.currentPower < 40)
                    playerPower.IncreasePower(1);
            spriteRenderer.flipX = false;
            sleepcount++;
        }

        // 시작 전, 잠든 상태, 폰 사용 중엔 이동 불가
        if (!start || animator.GetBool("isSleep") || animator.GetBool("isPhone"))
            return;

        // 이동
        rigid.velocity = new Vector2(h * maxSpeed, rigid.velocity.y);

        // 방향 전환
        if (h > 0)
            spriteRenderer.flipX = true;
        else if (h < 0)
            spriteRenderer.flipX = false;

        // 걷기 애니메이션
        animator.SetBool("isWalking", h != 0.0f);
    }

    IEnumerator DisableStartIndicator()
    {
        yield return new WaitForSeconds(0.2f);
        if (keyInfo != null)
            keyInfo.is_starting = false;
    }
    IEnumerator clickIndicator()
    {
        yield return new WaitForSeconds(2.0f);
        if (keyInfo != null)
            keyInfo.is_click = true;
    }
}
