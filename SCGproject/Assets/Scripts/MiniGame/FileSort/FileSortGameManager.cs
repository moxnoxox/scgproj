using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class FileSortGameManager : MonoBehaviour
{
    public static FileSortGameManager Instance;

    [Header("UI 연결")]
    public GameObject gameCanvasRoot;    // FileSortGameCanvas
    public GameObject tutorialPanel;
    private TextMeshProUGUI tutorialText;
    public TextMeshProUGUI timerText;    // TimerText
    public RectTransform fileParent;     // DesktopArea
    public GameObject fileIconPrefab;    // FileIcon 프리팹
    public TextMeshProUGUI feedbackText; // 게임 클리어 문구
    public Sprite[] thumbnailImages;
    Coroutine _feedbackRoutine;


    [Header("파일 데이터")]
    public List<FileData> fileDatabase = new List<FileData>();

    [Header("그리드 설정")]
    public int gridRows = 3;
    public int gridCols = 6;
    public float gridCellSize = 120f;

    private List<Vector2> gridPositions = new List<Vector2>();
    private float timer = 30f;
    private bool gameActive = false;

    private int totalCount;
    private int placedCorrectCount;

    Coroutine _timerHitRoutine;

    void Awake()
    {
        Instance = this;
        if (gameCanvasRoot) gameCanvasRoot.SetActive(false);
        tutorialText = tutorialPanel.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ShowGameUI()
    {
        gameCanvasRoot.SetActive(true);
        Time.timeScale = 1f; // UI 게임은 보통 1 유지
        StartCoroutine(StartCoroutine());
    }
    IEnumerator StartCoroutine()
    {
        yield return tutorialCoroutine();
        StartGame();
    }
    IEnumerator tutorialCoroutine()
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = "제한시간 30초 안에 파일을 정리해 보세요!";
        yield return new WaitForSeconds(2f);
        tutorialText.text = "각 파일을 해당하는 폴더로 드래그 앤 드롭하세요.\n잘못된 폴더에 놓으면 시간 패널티가 있습니다.\n일부 파일은 휴지통에 넣어야 합니다.\n\n마우스 클릭 시 시작됩니다!";
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Mouse0));
        tutorialPanel.SetActive(false);
    }
    void StartGame()
    {
        // 기존 아이콘 제거
        foreach (Transform child in fileParent)
            Destroy(child.gameObject);

        placedCorrectCount = 0;
        totalCount = fileDatabase.Count;

        GenerateGridPositions();

        // 모든 파일 생성
        foreach (var data in fileDatabase)
            CreateFileIcon(data);
        timer = 30f;
        gameActive = true;
        UpdateTimerView();
    }

    void GenerateGridPositions()
    {
        gridPositions.Clear();

        float startX = -((gridCols - 1) * gridCellSize) / 2f;
        float startY = ((gridRows - 1) * gridCellSize) / 2f;

        for (int r = 0; r < gridRows; r++)
        {
            for (int c = 0; c < gridCols; c++)
            {
                var pos = new Vector2(startX + c * gridCellSize, startY - r * gridCellSize);
                gridPositions.Add(pos);
            }
        }

        // 그리드 좌표 셔플
        for (int i = 0; i < gridPositions.Count; i++)
        {
            int j = Random.Range(i, gridPositions.Count);
            (gridPositions[i], gridPositions[j]) = (gridPositions[j], gridPositions[i]);
        }
    }

    void CreateFileIcon(FileData data)
    {
        if (gridPositions.Count == 0)
        {
            Debug.LogWarning("그리드 칸이 부족함. gridRows/gridCols 늘려줘");
            return;
        }

        var pos = gridPositions[0];
        gridPositions.RemoveAt(0);

        var go = Instantiate(fileIconPrefab, fileParent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;

        var item = go.GetComponent<DraggableItem>();
        item.SetFileData(data);   // 파일명/카테고리 세팅
        item.homeParent = fileParent; // 기본 부모는 DesktopArea
        item.rememberHomePosition = rect.anchoredPosition; // 초기 위치 저장
        if (data.extension == "png" || data.extension == "jpg" || data.extension == "jpeg")
        {
            item.GetComponent<UnityEngine.UI.Image>().sprite = thumbnailImages[0];
        }
        else if (data.extension == "mp3" || data.extension == "wav")
        {
            item.GetComponent<UnityEngine.UI.Image>().sprite = thumbnailImages[1];
        }
        else if (data.extension == "docx")
        {
            item.GetComponent<UnityEngine.UI.Image>().sprite = thumbnailImages[2];
        }
        else if (data.extension == "pptx")
        {
            item.GetComponent<UnityEngine.UI.Image>().sprite = thumbnailImages[3];
        }
        else if (data.extension == "pdf")
        {
            item.GetComponent<UnityEngine.UI.Image>().sprite = thumbnailImages[4];
        }
        
    }

    void Update()
    {
        if (!gameActive) return;

        UpdateTimerView();
    }

    void UpdateTimerView()
    {
        if (timerText) timerText.text = $"{timer:F1}";
    }

    void FixedUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 0;
            UpdateTimerView();

            // 타이머 종료 시점 처리
            EndGame(false);
            return;
        }
    }

    public void ApplyPenalty(float seconds)
    {
        if (!gameActive) return;        // 게임 중에만
        timer -= seconds;
        if (timer < 0) timer = 0;
        UpdateTimerView();
        PlayTimerHitFeedback();
    }

    void PlayTimerHitFeedback()
    {
        if (timerText == null) return;

        if (_timerHitRoutine != null)
            StopCoroutine(_timerHitRoutine);

        _timerHitRoutine = StartCoroutine(CoTimerHitFeedback());
    }

    IEnumerator CoTimerHitFeedback()
    {
        var t = 0f;
        var dur = 0.25f;
        var tf = timerText.transform;
        var rt = timerText.rectTransform;

        var baseScale = tf.localScale;
        var basePos = rt.anchoredPosition;
        var baseColor = timerText.color;

        float punchScale = 1.15f;
        float shakeAmp = 8f;
        Color flashColor = new Color(1f, 0.3f, 0.3f);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = t / dur;

            // 좌우 흔들림
            float w = Mathf.Sin(k * Mathf.PI * 6f);
            rt.anchoredPosition = basePos + new Vector2(w * shakeAmp * (1f - k), 0f);

            // 색상 플래시
            timerText.color = Color.Lerp(flashColor, baseColor, k);

            yield return null;
        }

        // 원상복구
        tf.localScale = baseScale;
        rt.anchoredPosition = basePos;
        timerText.color = baseColor;

        _timerHitRoutine = null;
    }

    public void NotifyPlacementResult(bool wasCorrect, bool becameCorrect)
    {
        if (becameCorrect && !wasCorrect)
            placedCorrectCount++;
        else if (!becameCorrect && wasCorrect)
            placedCorrectCount--;

        if (placedCorrectCount < 0)
            placedCorrectCount = 0;

    }


    void ShowFeedback(string text, Color color)
    {
        if (feedbackText == null) return;

        if (_feedbackRoutine != null)
            StopCoroutine(_feedbackRoutine);

        _feedbackRoutine = StartCoroutine(CoShowFeedback(text, color));
    }

    IEnumerator CoShowFeedback(string text, Color color)
    {
        feedbackText.text = text;
        feedbackText.color = new Color(color.r, color.g, color.b, 0f); // 알파 0으로 시작
        float t = 0f;

        // 페이드 인
        while (t < 0.2f)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(0, 1, t / 0.2f);
            feedbackText.color = new Color(color.r, color.g, color.b, a);
            yield return null;
        }

        // 유지
        yield return new WaitForSecondsRealtime(0.8f);

        // 페이드 아웃
        t = 0f;
        while (t < 0.3f)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(1, 0, t / 0.3f);
            feedbackText.color = new Color(color.r, color.g, color.b, a);
            yield return null;
        }

        _feedbackRoutine = null;
    }

    void EndGame(bool ignored)
    {
        gameActive = false;

        // 남은 시간 0으로 보정
        timer = 0;
        UpdateTimerView();

        // 모든 드래그 비활성화
        DisableAllDraggableItems();

        // 결과 판정
        if (placedCorrectCount == totalCount)
        {
            // 모든 파일 정리 완료
            ShowFeedback("PERFECT!", new Color(0.4f, 0.8f, 1f));
            Debug.Log("Perfect Clear!");
            Chapter2Manager.Instance.OnFileSortGameDone();
        }
        else if (placedCorrectCount > 0)
        {
            // 일부 정리 성공
            ShowFeedback("DONE!", new Color(1f, 0.9f, 0.3f));
            Debug.Log("Partial Clear (Done)");
            Chapter2Manager.Instance.OnFileSortGameDone();
        }
        else
        {
            Debug.Log("No files sorted.");
        }

        // 여기서 Chapter2Manager 연결할 수도 있음
        // ex) Chapter2Manager.Instance.OnFileSortGameDone();

        // 일정 시간 뒤 게임 닫기 (UI 연출용)
        StartCoroutine(CloseGame());
    }
    
    void DisableAllDraggableItems()
    {
        var items = FindObjectsOfType<DraggableItem>();
        foreach (var item in items)
        {
            var cg = item.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = false;
            }
        }
    }
    IEnumerator CloseGame()
    {
        yield return new WaitForSecondsRealtime(2f);
        gameCanvasRoot.SetActive(false);
        Time.timeScale = 1f;
    }

}
