using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MonologueManager_ch3 : MonoBehaviour
{
    public static MonologueManager_ch3 Instance;

    [Header("UI 연결")]
    public GameObject dialoguePanel;           // 대사 패널
    public TextMeshProUGUI nameText;           // 화자 이름
    public TextMeshProUGUI dialogueText;       // 대사 텍스트
    public TextMeshProUGUI announcementText;   // “안내 문구”용 텍스트

    private RectTransform panelTransform;
    private Vector3 originalPos;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (dialoguePanel != null)
            panelTransform = dialoguePanel.GetComponent<RectTransform>();

        if (panelTransform != null)
            originalPos = panelTransform.anchoredPosition;
    }

    // ==========================================
    // 일반 대사 (이름 + 텍스트 분리)
    // ==========================================
    public IEnumerator ShowDialogueLines(List<DialogueLine> lines, float showTime = 2f, bool shake = false)
    {
        if (dialoguePanel == null || nameText == null || dialogueText == null)
            yield break;

        dialoguePanel.SetActive(false);
        yield return null; // 한 프레임 쉬고
        dialoguePanel.SetActive(true);

        nameText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);

        if (shake)
            StartCoroutine(ShakePanel(0.3f, 10f));

        foreach (var line in lines)
        {
            nameText.text = line.speaker;
            dialogueText.text = line.text;
            yield return new WaitForSeconds(showTime);
        }

        dialoguePanel.SetActive(false);
    }

    // ==========================================
    // 독백 (이름 없이 텍스트만)
    // ==========================================
    public IEnumerator ShowMonologueLines(List<string> lines, float showTime = 2f, bool shake = false)
    {
        if (dialoguePanel == null || dialogueText == null)
            yield break;

        dialoguePanel.SetActive(true);
        nameText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(true);

        if (shake)
            StartCoroutine(ShakePanel(0.3f, 10f));

        foreach (var line in lines)
        {
            dialogueText.text = line;
            yield return new WaitForSeconds(showTime);
        }

        dialoguePanel.SetActive(false);
    }

    // ==========================================
    // 안내 문구 (예: "자료 수정 중...")
    // ==========================================
    public void ShowAnnouncement(List<string> messages, float duration = 3f, bool shake = false)
    {
        if (messages == null || messages.Count == 0 || announcementText == null)
            return;

        StartCoroutine(ShowAnnouncementRoutine(messages, duration, shake));
    }

    private IEnumerator ShowAnnouncementRoutine(List<string> messages, float duration, bool shake)
    {
        if (announcementText == null)
            yield break;

        announcementText.gameObject.SetActive(true);

        if (shake)
            StartCoroutine(ShakePanel(0.2f, 5f));

        foreach (var line in messages)
        {
            announcementText.text = line;
            yield return new WaitForSeconds(duration);
        }

        announcementText.gameObject.SetActive(false);
    }

    // ==========================================
    // 흔들림 효과
    // ==========================================
    private IEnumerator ShakePanel(float duration, float magnitude)
    {
        if (panelTransform == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            panelTransform.anchoredPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        panelTransform.anchoredPosition = originalPos;
    }
}
