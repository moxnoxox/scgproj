using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager_ch3 : MonoBehaviour
{
    public static GameManager_ch3 Instance;

    [Header("참조 오브젝트")]
    public PlayerMove_ch3 playerMove;
    public Transform choicePanel;
    public GameObject choiceButtonPrefab;
    public Image backgroundImage;
    public Image illustrationImage;   // 일러스트 표시용
    public Image fadeImage;           // 암전용 이미지 (검은색, 알파 0)
    public MonologueManager_ch3 monologueManager;
    public player_power_ch3 playerPower;

    [Header("NPC 및 트리거")]
    public Transform npcTarget;
    public float triggerDistance = 2f;

    // JSON 캐싱(Wrapper → Dict)
    private Dictionary<string, List<DialogueLine>> monoData;

    // 선택지 상태
    private bool choiceSelected;
    private int selectedIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        LoadMonoJson();
        StartCoroutine(ScenarioFlow());
    }

    // =========================
    // JSON 로드 (Wrapper 방식)
    // =========================
    private void LoadMonoJson()
    {
        TextAsset monoJson = Resources.Load<TextAsset>("MonologueData/Mono3");
        if (monoJson == null)
        {
            Debug.LogError("[GameManager_ch3] Mono3.json 파일을 찾을 수 없습니다 (Resources/MonologueData/Mono3.json)");
            monoData = new Dictionary<string, List<DialogueLine>>();
            return;
        }

        try
        {
            var wrapper = JsonUtility.FromJson<MonoDataWrapper>(monoJson.text);
            if (wrapper == null)
            {
                Debug.LogError("[GameManager_ch3] JsonUtility 파싱 실패 (wrapper == null)");
                monoData = new Dictionary<string, List<DialogueLine>>();
                return;
            }

            monoData = wrapper.ToDictionary();
            Debug.Log($"[GameManager_ch3] JSON 로드 완료. 키 개수: {monoData.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameManager_ch3] JSON 로드 중 예외: {e.Message}");
            monoData = new Dictionary<string, List<DialogueLine>>();
        }
    }

    // =========================
    // 시나리오 흐름
    // =========================
    private IEnumerator ScenarioFlow()
    {
        // 초기 상태
        playerMove.movable = false;
        playerMove.canInput = false;
        if (illustrationImage) illustrationImage.gameObject.SetActive(false);
        if (fadeImage)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        // 1) 시작 2초 후 첫 대사
        yield return new WaitForSeconds(2f);
        yield return Show("intro_1", 3f);

        // 플레이어 이동 허용
        playerMove.movable = true;
        playerMove.canInput = true;

        // 2) NPC 접근까지 대기
        yield return new WaitUntil(() =>
            npcTarget != null &&
            Vector2.Distance(playerMove.transform.position, npcTarget.position) <= triggerDistance
        );

        // 입력 잠금
        playerMove.movable = false;
        playerMove.canInput = false;

        // 3) 만남 대사 + 일러스트 + 대사
        StartCoroutine(ShowIllustration("meetguitar"));
        yield return Show("meet_1", 4.5f);
        yield return Show("meet_2", 3f);

        // 4) 기타 전달
        yield return StartCoroutine(ShowIllustration("guitar_fixed_illust"));
        yield return Show("giveGuitar", 3f);

        // 5) 기타 돌려줌
        yield return StartCoroutine(ShowIllustration("guitar_fixed_illust"));
        yield return Show("giveGuitar_2", 3f);

        // 6) 에너지 변동
        if (playerPower != null) playerPower.IncreasePower(20);
        yield return Show("filledEnergy", 3f);

        // 7) 소리치는 대사(흔들림) → 후속 대사
        yield return Show("shout", 2f, shake:true);
        yield return Show("afterShout", 3f);

        // 8) 암전 → 배경 전환
        yield return StartCoroutine(FadeOut(1.5f));
        yield return new WaitForSeconds(1.5f);
        SetBackgroundToIllustration("End");
        yield return StartCoroutine(FadeIn(1.5f));

        // 9) 대사
        yield return Show("bus", 3f);

        // 10) 선택지
        yield return ShowChoices(new List<string>
        {
            "> 모두와 함께 연습했던 <b>연습 스튜디오</b>",
            "> 첫 버스킹했던 <b>바다</b>",
            "> 발길 끊었던 <b>상담센터</b>",
        });
        int result = GetChoiceResult();
        PlayerPrefs.SetInt("ch3_choiceResult", result);

        // 11) 마무리 대사
        yield return Show("afterChoose", 3f);
    }

    // 편의 함수: 키로 대사 출력
    private IEnumerator Show(string key, float showTime, bool shake = false)
    {
        if (monoData == null || !monoData.ContainsKey(key) || monoData[key] == null || monoData[key].Count == 0)
        {
            Debug.LogWarning($"[GameManager_ch3] 키 '{key}' 없음 혹은 빈 배열");
            yield break;
        }
        yield return StartCoroutine(monologueManager.ShowDialogueLines(monoData[key], showTime, shake));
    }

    // =========================
    // 일러스트 & 페이드
    // =========================
    private IEnumerator ShowIllustration(string illustName)
    {
        if (illustrationImage == null) yield break;

        Sprite sprite = Resources.Load<Sprite>($"Illustrations/{illustName}");
        if (sprite == null)
        {
            Debug.LogWarning($"일러스트 {illustName}을(를) 찾을 수 없음");
            yield break;
        }

        illustrationImage.sprite = sprite;
        illustrationImage.SetNativeSize();                      // 원본 크기로 맞추기
        illustrationImage.rectTransform.anchoredPosition = Vector2.zero; // 중앙 정렬
        illustrationImage.color = new Color(1, 1, 1, 0);
        illustrationImage.gameObject.SetActive(true);

        // 🔹 페이드인
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            illustrationImage.color = new Color(1, 1, 1, Mathf.Clamp01(t));
            yield return null;
        }

        // 🔹 3초 유지
        yield return new WaitForSeconds(3f);

        // 🔹 페이드아웃
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            float alpha = 1 - Mathf.Clamp01(t);
            illustrationImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        // 🔹 비활성화
        illustrationImage.gameObject.SetActive(false);
    }


    private void SetBackgroundToIllustration(string illustName)
    {
        if (backgroundImage == null) return;

        Sprite sprite = Resources.Load<Sprite>($"Illustrations/{illustName}");
        if (sprite == null) return;

        backgroundImage.sprite = sprite;
        backgroundImage.color = Color.white;
        backgroundImage.SetNativeSize();
        backgroundImage.rectTransform.anchoredPosition = Vector2.zero;
        backgroundImage.gameObject.SetActive(true);

        if (illustrationImage) illustrationImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut(float duration)
    {
        if (fadeImage == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(t / duration));
            yield return null;
        }
    }

    private IEnumerator FadeIn(float duration)
    {
        if (fadeImage == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - Mathf.Clamp01(t / duration));
            yield return null;
        }
    }

    // =========================
    // 선택지
    // =========================
    private IEnumerator ShowChoices(List<string> options)
    {
        choiceSelected = false;
        selectedIndex = -1;

        if (monologueManager != null && monologueManager.dialoguePanel != null)
        {
            monologueManager.dialoguePanel.SetActive(true);
            if (monologueManager.nameText != null)
                monologueManager.nameText.gameObject.SetActive(true);
            if (monologueManager.dialogueText != null)
                monologueManager.dialogueText.gameObject.SetActive(false);
        }

        foreach (Transform child in choicePanel)
            Destroy(child.gameObject);

        choicePanel.gameObject.SetActive(true);

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicePanel);

            // 🔹 프리팹이 비활성화 상태로 복제된 경우 강제 활성화
            btnObj.SetActive(true);

            // 🔹 Button과 TMP 강제 Enable
            var button = btnObj.GetComponent<Button>();
            if (button != null) button.enabled = true;

            var text = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.enabled = true;
                text.gameObject.SetActive(true);
                text.text = options[i];
            }

            if (button != null)
                button.onClick.AddListener(() => OnChoiceSelected(index));
        }

        yield return new WaitUntil(() => choiceSelected);

        foreach (Transform child in choicePanel)
            Destroy(child.gameObject);

        choicePanel.gameObject.SetActive(false);

        if (monologueManager != null && monologueManager.dialoguePanel != null)
            monologueManager.dialoguePanel.SetActive(false);
    }


    private void OnChoiceSelected(int index)
    {
        Debug.Log($"[선택지 클릭 감지됨] index = {index}");
        selectedIndex = index;
        choiceSelected = true;
    }

    private int GetChoiceResult()
    {
        return selectedIndex;
    }
}

// ----------------------------
// JSON Wrapper & Line 타입
// ----------------------------
[Serializable]
public class MonoDataWrapper
{
    public List<DialogueLine> intro_1;
    public List<DialogueLine> meet_1;
    public List<DialogueLine> meet_2;
    public List<DialogueLine> giveGuitar;
    public List<DialogueLine> giveGuitar_2;
    public List<DialogueLine> filledEnergy;
    public List<DialogueLine> shout;
    public List<DialogueLine> afterShout;
    public List<DialogueLine> bus;
    public List<DialogueLine> afterChoose;

    public Dictionary<string, List<DialogueLine>> ToDictionary()
    {
        var d = new Dictionary<string, List<DialogueLine>>();
        if (intro_1 != null) d["intro_1"] = intro_1;
        if (meet_1 != null) d["meet_1"] = meet_1;
        if (meet_2 != null) d["meet_2"] = meet_2;
        if (giveGuitar != null) d["giveGuitar"] = giveGuitar;
        if (giveGuitar_2 != null) d["giveGuitar_2"] = giveGuitar_2;
        if (filledEnergy != null) d["filledEnergy"] = filledEnergy;
        if (shout != null) d["shout"] = shout;
        if (afterShout != null) d["afterShout"] = afterShout;
        if (bus != null) d["bus"] = bus;
        if (afterChoose != null) d["afterChoose"] = afterChoose;
        return d;
    }
}

[Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
}
