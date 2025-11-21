using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class computer : MonoBehaviour, IInteractable
{
    public GameObject player;
    public SpriteRenderer spriteRenderer;
    public Sprite computerOn;
    public Sprite computerOff;
    public player_power playerPower;
    public key_info keyInfo;
    public Image home;

    public bool isHomeClosed = false;   // ✅ 홈 화면 닫힘 상태
    private bool isPlayerNear = false;
    private bool hasBeenInteracted = false;
    private bool firstChecked = false;  // ✅ 최초 1회 긴 대사 플래그


    void Start()
    {
        if (home != null) home.enabled = false;

        if (player != null)
            playerPower = player.GetComponent<player_power>();
    }

    void Update()
    {
        if (player == null) return;

        float xdiff = Mathf.Abs(transform.position.x - player.transform.position.x);
        bool currentlyNear = xdiff < 1f;

        if (currentlyNear != isPlayerNear)
        {
            isPlayerNear = currentlyNear;
            spriteRenderer.sprite = isPlayerNear ? computerOn : computerOff;

            if (keyInfo != null)
                keyInfo.isObject = isPlayerNear && !hasBeenInteracted;
        }
    }

    public void Interact(PlayerMove player)
    {
        if (!isPlayerNear) return;
        if(!GameManager.Instance.spacable) return;
        hasBeenInteracted = true;
        if (keyInfo != null)
            keyInfo.isObject = false;

        // ✅ 최초 1회만 긴 대사, 이후엔 짧은 대사
        if (!firstChecked)
        {
            firstChecked = true;
            StartCoroutine(FirstCheckRoutine());
        }
        else
        {
            StartCoroutine(ShowHomeImage());
        }

        GameManager.Instance?.onComputerChecked();
    }

    // ✅ 최초 1회 긴 대사 루틴
    private IEnumerator FirstCheckRoutine()
    {
        InputBlocker.Enable();
        player.GetComponent<PlayerMove>().movable = false;
        player.GetComponent<PlayerMove>().canInput = false;
        isHomeClosed = false;
        GameManager.Instance.spacable = false;
        if (home != null) home.enabled = true;

        // 첫 번째 긴 대사 (끝날 때까지 기다림)
        yield return StartCoroutine(MonologueManager.Instance.ShowMonologueRoutine(new List<string>
        {
            "윽…화면이 왜 이렇게 더럽지.",
            "정리 안 한 지 이렇게 오래 됐었나?",
            "안 그래도 보기 싫은데, 이러면 더 보기 싫어지잖아…",
            "하…… 아냐. 그만 회피하고 빨리 끝내자."
        }, 3f));

        // “자료 수정 중...” 표시
        yield return StartCoroutine(MonologueManager.Instance.ShowMonologueRoutine(new List<string>
        {
            "(자료 수정 중…)"
        }, 5f));

        // 완료 멘트
        yield return StartCoroutine(MonologueManager.Instance.ShowMonologueRoutine(new List<string>
        {
            "됐다… 겨우 다 했네.",
            "벌써 2시간이나 흘렀네.",
            "이제 침대에 눕고 싶어. 근데도 할 일이 남았다니…"
        }, 3f));

        if (home != null) home.enabled = false;
        isHomeClosed = true;
        InputBlocker.Disable();  
        player.GetComponent<PlayerMove>().movable = true;
        player.GetComponent<PlayerMove>().canInput = true;
        GameManager.Instance.spacable = true;
        if (playerPower != null) playerPower.DecreasePower(10);
    }



    // ✅ 이후 짧은 멘트용 루틴
    private IEnumerator ShowHomeImage()
    {
        InputBlocker.Enable();
        player.GetComponent<PlayerMove>().movable = false;
        player.GetComponent<PlayerMove>().canInput = false;
        isHomeClosed = false;
        if (home != null) home.enabled = true;
        GameManager.Instance.spacable = false;

        MonologueManager.Instance.ShowMonologuesSequentially(new List<string>{"파일 정리하기 너무 귀찮아..."}, 3f);

        yield return new WaitForSeconds(3f);

        if (home != null) home.enabled = false;
        isHomeClosed = true;
        InputBlocker.Disable();
        player.GetComponent<PlayerMove>().movable = true;
        player.GetComponent<PlayerMove>().canInput = true;
        GameManager.Instance.spacable = true;
        if (playerPower != null) playerPower.DecreasePower(10);
    }
}
