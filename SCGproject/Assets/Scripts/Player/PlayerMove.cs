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
    [SerializeField] float h = 0f;
    public player_power playerPower;
    public GameObject key_info;
    public GameManager gameManager;
    public CanvasGroup canvas;
    public bool movable = true;
    private key_info keyInfo;
    public bool isHolding = false;
    public GameObject heldObject = null;
    bool showedTiredDialogue = false;
    private bool hasLiedDown = false;   
    private IInteractable interactionTarget = null;
    private string currentScene;
    // 자동리턴 관련 
    public bool autoMoveActive = false;
    private bool isAutoReturnRunning = false;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();
        animator.SetBool("isWalking", false);
        animator.SetBool("isPhone", false);
        
        currentScene = SceneManager.GetActiveScene().name;

        if (key_info != null)keyInfo = key_info.GetComponent<key_info>();

        if (currentScene == "Chapter1")
        {
            if (keyInfo != null)
                keyInfo.isBed = true;
        }

        // ✅ 씬별 초기 상태 설정
        if (currentScene == "Chapter2")
        {
            animator.SetBool("isSleep", false);
            start = true;
        }
        else
        {
            animator.SetBool("isSleep", true);
            start = false;
            if (keyInfo != null) keyInfo.isBed = true;
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
        if (animator.GetBool("isSleep")) return;   // 이미 수면 상태면 무시

        animator.SetBool("isWalking", false);      // 걷기 끄기
        animator.SetBool("isSleep", true);         // 수면 on
        rigid.linearVelocity = Vector2.zero;             // 완전 정지
        hasLiedDown = true;
    }


    void Update()
    {
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isHolding)
            {
                DropHeldObject();
            }
            else if (interactionTarget != null)
            {
                interactionTarget.Interact(this);
            }
        }
        if(currentScene == "Chapter2")
        {
            if (Chapter2Manager.Instance != null)
             movable = Chapter2Manager.Instance.ch2_movable;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactionTarget = interactable;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactionTarget == interactable)
        {
            interactionTarget = null;
        }
    }

    IEnumerator StartWait()
    {
        yield return new WaitForSeconds(1.0f);
        start = true;
        animator.SetBool("isSleep", false);
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

        if (!start && !starting && animator.GetBool("isSleep") == false && currentScene == "Chapter1")
        {
            starting = true;
            animator.SetBool("start_trigger", true);
            StartCoroutine(StartWait());
        }

        if (start && h != 0)
        {
            if (!hasMoved)
            {
                if (currentScene == "Chapter1" && gameManager != null)
                    gameManager.OnPlayerMoved();

                hasMoved = true;

                if (startIndicatorCoroutine != null)
                    StopCoroutine(startIndicatorCoroutine);
                startIndicatorCoroutine = StartCoroutine(DisableStartIndicator());
            }
        }
      // ★ 클래스 필드 autoMoveActive를 그대로 사용하고,
        //    Chapter2Manager의 autoMove가 켜져 있으면 함께 활성화
        bool autoMoveNow = autoMoveActive 
                           || (currentScene == "Chapter2" 
                               && Chapter2Manager.Instance != null 
                               && Chapter2Manager.Instance.autoMove);

        if (autoMoveNow)
        {
            canInput = false;

            if (sleepcount == 0)
                StartCoroutine(clickIndicator());

            // 침대가 x=0 기준이라고 가정한 자동 이동
            h = -transform.position.x;
            if (h > 0.1f) h = 1;
            else if (h < -0.1f) h = -1;
            else h = 0;

            rigid.linearVelocity = new Vector2(h * maxSpeed, rigid.linearVelocity.y);

            // ✅ 도착 판정 범위 여유 + 눕기 지연
            if (Mathf.Abs(transform.position.x) <= 0.2f && !hasLiedDown)
            {
                StartCoroutine(LieDownDelay());
            }
        }

        if (currentScene == "Chapter1" && keyInfo != null)
        {
            if (keyInfo.is_click && animator.GetBool("isPhone") == true)
            {
                keyInfo.is_click = false;
            }
        }

        if (animator.GetBool("isSleep"))
        {
            if (playerPower != null)
                if (playerPower.currentPower < 40)
                    playerPower.IncreasePower(1);
            spriteRenderer.flipX = false;
            sleepcount++;
        }

        if (!start || animator.GetBool("isSleep") || animator.GetBool("isPhone"))
            return;

        rigid.linearVelocity = new Vector2(h * maxSpeed, rigid.linearVelocity.y);

        if (h > 0)
            spriteRenderer.flipX = true;
        else if (h < 0)
            spriteRenderer.flipX = false;

        animator.SetBool("isWalking", h != 0.0f);
        animator.SetBool("isHolding", isHolding);

        if (isHolding && heldObject != null)
        {
            Vector3 holdPos = transform.position + new Vector3(0.1f * (spriteRenderer.flipX ? 1 : -1), 0.001f, 0);
            heldObject.transform.position = holdPos;
        }
    }
    public IEnumerator AutoReturnToBed(bool skipDialogue)
    {
        if (isAutoReturnRunning) yield break;
        isAutoReturnRunning = true;
        canInput = false;
        hasLiedDown = false;

        if (!skipDialogue)
        {
            List<string> beforeLines = new List<string> { "너무 피곤해… 눕고 싶어." };
            MonologueManager.Instance.ShowMonologuesSequentially(beforeLines, 2f);
            yield return new WaitForSecondsRealtime(beforeLines.Count * 2f + 0.5f);
        }

        autoMoveActive = true;
        Debug.Log("★ 자동 리턴 시작");

        yield return new WaitUntil(() => animator.GetBool("isSleep"));
        autoMoveActive = false;

        if (!skipDialogue)
        {
            List<string> afterLines = new List<string> { "편하다… 하루종일 침대에만 붙어 있고 싶어." };
            MonologueManager.Instance.ShowMonologuesSequentially(afterLines, 2f);
            yield return new WaitForSecondsRealtime(afterLines.Count * 2f + 0.5f);
        }

        isAutoReturnRunning = false;
        canInput = true;
        Debug.Log("★ 자동 리턴 종료");
    }

    private IEnumerator LieDownDelay()
    {
        if (hasLiedDown) yield break;
        hasLiedDown = true;
        rigid.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.15f);
        SleepExternal();
        Debug.Log("침대 도착 → 눕기 실행(지연)");
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
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        StartCoroutine(ReenableCollider(heldObject));

        Vector3 dropPos = transform.position + new Vector3(spriteRenderer.flipX ? 0.6f : -0.6f, -0.25f, 0);
        heldObject.transform.position = dropPos;
        heldObject.transform.parent = null;

        // 💡 플레이어가 쓰레기통 안에 있으면 즉시 삭제
        if (TrashCan.PlayerInside)
        {
            Debug.Log($"TrashCan 범위 안에서 '{heldObject.name}'을(를) 버림. 즉시 파괴.");
            Destroy(heldObject);
        }

        isHolding = false;
        heldObject = null;

        Debug.Log("DropHeldObject: Object dropped.");
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
