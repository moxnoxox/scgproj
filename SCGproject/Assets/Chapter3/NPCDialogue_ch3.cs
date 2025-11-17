using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue_ch3 : MonoBehaviour
{
    [Header("참조")]
    public PlayerMove_ch3 playerMove;
    public MonologueManager_ch3 monologueManager;
    public GameManager_ch3 gameManager;

    [Header("상호작용 설정")]
    public float talkDistance = 1.5f;
    public bool oneTimeOnly = true;

    [Header("대사 (선택지 전)")]
    public List<DialogueLine> preChoiceLines = new List<DialogueLine>();

    [Header("선택지 설정")]
    public bool useChoice = false;
    public string choiceText = "선택지 문구";

    [Header("대사 (선택지 후)")]
    public List<DialogueLine> afterChoiceLines = new List<DialogueLine>();

    private bool hasTalked = false;
    private Transform playerTr;

    void Start()
    {
        if (playerMove == null && GameManager_ch3.Instance != null)
            playerMove = GameManager_ch3.Instance.playerMove;

        if (monologueManager == null && GameManager_ch3.Instance != null)
            monologueManager = GameManager_ch3.Instance.monologueManager;

        if (gameManager == null && GameManager_ch3.Instance != null)
            gameManager = GameManager_ch3.Instance;

        if (playerMove != null)
            playerTr = playerMove.transform;
    }

    void Update()
    {
        if (playerTr == null) return;

        float dist = Vector2.Distance(transform.position, playerTr.position);

        if (dist <= talkDistance &&
            Input.GetKeyDown(KeyCode.Space) &&
            (!oneTimeOnly || !hasTalked))
        {
            StartCoroutine(TalkRoutine());
        }
    }

    IEnumerator TalkRoutine()
    {
        hasTalked = true;

        playerMove.movable = false;
        playerMove.canInput = false;
        var rigid = playerMove.GetComponent<Rigidbody2D>();
        if (rigid != null) rigid.linearVelocity = Vector2.zero;

        // ***** 1) 선택지 전 대사 *****
        if (preChoiceLines.Count > 0)
            yield return StartCoroutine(monologueManager.ShowDialogueLines(preChoiceLines, 2.2f));

        // ***** 2) 선택지 *****
        if (useChoice)
        {
            // 하나짜리 선택지만 사용
            yield return StartCoroutine(gameManager.ShowChoices(new List<string> { choiceText }));
            int r = gameManager.GetChoiceResult();
            // 하나뿐이므로 r == 0
        }

        // ***** 3) 선택지 후 대사 *****
        if (afterChoiceLines.Count > 0)
            yield return StartCoroutine(monologueManager.ShowDialogueLines(afterChoiceLines, 2.2f));

        // ***** 이동 복원 *****
        playerMove.movable = true;
        playerMove.canInput = true;
    }
}
