using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileSortGameManager : MonoBehaviour
{
    public static FileSortGameManager Instance;

    [Header("Wiring")]
    public GameObject gameCanvasRoot; // FileSortGameCanvas
    public Button exitButton;         // 완료 후 활성화
    public Button completeButton;     // 모든 배치 완료 시 노출/활성
    public List<DraggableItem> allItems = new List<DraggableItem>();

    int totalCount;
    int correctCount;
    bool active;

    void Awake()
    {
        Instance = this;
        if (gameCanvasRoot != null) gameCanvasRoot.SetActive(false);
        if (exitButton != null) exitButton.interactable = false;
        if (completeButton != null) completeButton.gameObject.SetActive(false);
    }

    public void ShowGameUI()
    {
        active = true;
        totalCount = allItems.Count;
        correctCount = 0;

        // 초기화
        foreach (var it in allItems)
        {
            it.isCorrectlyPlaced = false;
        }

        gameCanvasRoot.SetActive(true);
        Time.timeScale = 0f;

        if (exitButton) exitButton.interactable = false;
        if (completeButton) { completeButton.gameObject.SetActive(false); completeButton.interactable = false; }
    }

    public void HideGameUI()
    {
        active = false;
        gameCanvasRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    // 슬롯이 호출
    public void NotifyPlacementChanged(DraggableItem item, bool isCorrectNow)
    {
        if (isCorrectNow)
            correctCount++;
        else
            correctCount = Mathf.Max(0, correctCount - 1);

        CheckComplete();
    }

    void CheckComplete()
    {
        if (!active) return;

        if (correctCount >= totalCount)
        {
            if (completeButton)
            {
                completeButton.gameObject.SetActive(true);
                completeButton.interactable = true;
            }
            if (exitButton) exitButton.interactable = true;
        }
    }

    // UI 버튼에서 연결
    public void OnClickComplete()
    {
        // 최종 완료 처리
        HideGameUI();
        Chapter2Manager.Instance.OnFileSortGameDone();
    }

    public void OnClickExit()
    {
        // 미완료 상태라면 경고창을 띄우거나 무시하도록 네가 결정
        HideGameUI();
    }
}
