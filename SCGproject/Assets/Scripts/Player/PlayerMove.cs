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
    
    private IInteractable interactionTarget = null;

    private string currentScene;

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
        if (!animator.GetBool("isSleep"))
        {
            animator.SetBool("isSleep", true);
        }
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

        bool autoMoveActive = false;
        if (currentScene == "Chapter1")
        {
            if (GameManager.Instance != null && GameManager.Instance.autoMove)
                autoMoveActive = true;
        }
        else if (currentScene == "Chapter2")
        {
            if (Chapter2Manager.Instance != null && Chapter2Manager.Instance.autoMove)
                autoMoveActive = true;
        }

        if (autoMoveActive && playerPower != null && playerPower.noPower)
        {
            if (sleepcount == 0)
                StartCoroutine(clickIndicator());

            h = -transform.position.x;
            if (h > 0.1f) h = 1;
            else if (h < -0.1f) h = -1;
            else h = 0;

            rigid.linearVelocity = new Vector2(h * maxSpeed, rigid.linearVelocity.y);
        }
        if (currentScene == "Chapter1" && keyInfo != null) {
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
