using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed = 5f;
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    public Animator animator;
    private bool start = false;
    public bool starting = false;
    public bool canInput = false;
    private int sleepcount = 0;
    private float h = 0f;
    public player_power playerPower;
    public key_info keyInfo;
    public GameManager gameManager;
    public CanvasGroup canvas;
    public bool movable = true;

    public bool isHolding = false;
    public GameObject heldObject = null;
    public GameObject pickupTarget = null; // 💡 근처의 들 수 있는 오브젝트

    // 현재 씬 이름 저장용 (필드 선언만, 초기화는 Awake에서)
    private string currentScene;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();
        animator.SetBool("isWalking", false);
        animator.SetBool("isPhone", false);
        keyInfo.isBed = true;

        // 여기서만 SceneManager 호출 (UnityException 방지)
        currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Chapter2") // 챕터2 씬 이름 정확히 입력
        {
            // 챕터2: 바로 일어난 상태로 시작
            animator.SetBool("isSleep", false);
            start = true;
            keyInfo.isBed = false;
        }
        else
        {
            // 챕터1: 누워서 시작
            animator.SetBool("isSleep", true);
            start = false;
            keyInfo.isBed = true;
        }
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

        // 💡 Space 입력 처리 (PlayerMove만 담당)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isHolding)
            {
                DropHeldObject();
            }
            else if (pickupTarget != null)
            {
                // 근처 오브젝트의 TryLift 호출
                var one = pickupTarget.GetComponent<TrashBag1Stack>();
                var two = pickupTarget.GetComponent<TrashBag2Stack>();

                if (one != null)
                    one.TryLift(this);
                else if (two != null)
                    two.TryLift(this);
            }
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
        if (movable)
        {
            h = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            h = 0;
        }
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
            if (!hasMoved)
            {
                // 현재 씬이 Chapter1(SampleScene)이고, gameManager가 연결돼 있을 때만 호출
                if (currentScene == "SampleScene" && gameManager != null)
                    gameManager.OnPlayerMoved();

                hasMoved = true;

                if (startIndicatorCoroutine != null)
                    StopCoroutine(startIndicatorCoroutine);
                startIndicatorCoroutine = StartCoroutine(DisableStartIndicator());
            }
        }

        // 파워가 없으면 침대로 이동
        bool autoMoveActive = false;

        // 씬 이름으로 먼저 분기 → 해당 매니저만 접근
        if (currentScene == "SampleScene")
        {
            if (GameManager.Instance != null && GameManager.Instance.autoMove)
                autoMoveActive = true;
        }
        else if (currentScene == "Chapter2")
        {
            if (Chapter2Manager.Instance != null && Chapter2Manager.Instance.autoMove)
                autoMoveActive = true;
        }

        // 자동 이동 처리
        if (autoMoveActive && playerPower != null && playerPower.noPower)
        {
            if (sleepcount == 0)
                StartCoroutine(clickIndicator());

            // 이미 위에서 선언된 h 사용
            h = -transform.position.x;
            if (h > 0.1f) h = 1;
            else if (h < -0.1f) h = -1;
            else h = 0;

            // 이동 적용
            rigid.velocity = new Vector2(h * maxSpeed, rigid.velocity.y);
        }

        if (keyInfo.is_click && animator.GetBool("isPhone") == true)
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

        // 들고 걷기 애니메이션
        animator.SetBool("isHolding", isHolding);

        // 들고 있는 오브젝트가 있다면 따라오게 하기
        if (isHolding && heldObject != null)
        {
            Vector3 holdPos = transform.position + new Vector3(0.8f * (spriteRenderer.flipX ? 1 : -1), 0.3f, 0);
            heldObject.transform.position = holdPos;
        }
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
    public void showClickIndicator()
    {
        StartCoroutine(clickIndicator());
    }

    //쓰봉 들기
    public void DropHeldObject()
    {
        if (heldObject == null) return;

        if (animator != null)
            animator.SetTrigger("hold_end");

        Rigidbody2D rb = heldObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Collider 다시 켜기 (지연 실행)
        StartCoroutine(ReenableCollider(heldObject));

        // 플레이어 앞에 놓기
        Vector3 dropPos = transform.position + new Vector3(spriteRenderer.flipX ? 1.2f : -1.2f, -0.2f, 0);
        heldObject.transform.position = dropPos;
        heldObject.transform.parent = null;

        TrashBag1Stack.ResetHeldStatus();

        isHolding = false;
        heldObject = null;
        pickupTarget = null;

        Debug.Log(" DropHeldObject 추가: 바닥에 내려놓음");
    }

    private IEnumerator ReenableCollider(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);
        if (obj == null) yield break;

        Collider2D[] cols = obj.GetComponents<Collider2D>();
        foreach (var col in cols)
            col.enabled = true;
    }
}
