using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;

public class PaperpuzzleController : MonoBehaviour
{
    public static PaperpuzzleController Instance;
    public GameObject[] pieces;
    public GameObject tutorialPanel;
    private TextMeshProUGUI tutorialText;
    public PaperHandler[] pieceHandlers;
    public bool isPuzzleActive;
    public CanvasGroup puzzlePanel;
    public UnityEngine.UI.Image completeImage;
    public TextMeshProUGUI completeText;
    public bool isCompleted;
    void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        puzzlePanel.interactable = false;
        puzzlePanel.blocksRaycasts = false;
        puzzlePanel.alpha = 0;
        isPuzzleActive = false;
        isCompleted = false;
        for (int i = 0; i < 9; i++)
        {
            pieceHandlers[i] = pieces[i].GetComponent<PaperHandler>();
        }
        tutorialText = tutorialPanel.GetComponentInChildren<TextMeshProUGUI>();
        tutorialPanel.SetActive(false);
        puzzlePanel.alpha = 0;
        puzzlePanel.interactable = false;
        puzzlePanel.blocksRaycasts = false;
        completeImage.gameObject.SetActive(false);
        completeText.gameObject.SetActive(false);
        //디버그용
        //StartPuzzle();
    }
    public void StartPuzzle()
    {
        StartCoroutine(PuzzleRoutine());
    }
    public void OnExitButton()
    {
        isPuzzleActive = false;
        puzzlePanel.interactable = false;
        puzzlePanel.blocksRaycasts = false;
        puzzlePanel.alpha = 0;
        //reset puzzle
        foreach (var handler in pieceHandlers)
        {
            handler.transform.position = handler.originalPosition;
            handler.transform.rotation = handler.originalRotation;
            handler.isCorrectPosition = false;
            handler.currentSnappedIndex = -1;
        }
        Chapter2Manager.Instance.ch2_movable = true;
    }
    public void OnResetButton()
    {
        foreach (var handler in pieceHandlers)
        {
            handler.transform.position = handler.originalPosition;
            handler.transform.rotation = handler.originalRotation;
            handler.isCorrectPosition = false;
            handler.currentSnappedIndex = -1;
        }
        isCompleted = false;
    }
    IEnumerator tutorialRoutine()
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = "찢긴 종잇조각들이 흩어져 있습니다. 모양을 잘 보고 이어 붙여 보세요!";
        yield return new WaitForSeconds(2f);
        tutorialText.text = "조각을 드래그하여 맞는 위치에 놓으세요.\n조각을 더블클릭하면 회전할 수 있습니다.\n\n마우스 클릭 시 시작됩니다!";
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Mouse0));
        tutorialPanel.SetActive(false);
    }

    IEnumerator PuzzleRoutine()
    {
        
        puzzlePanel.interactable = true;
        puzzlePanel.blocksRaycasts = true;
        puzzlePanel.alpha = 1;
        yield return tutorialRoutine();
        // 1. 게임 안내 멘트 출력

        // 2. 게임 진행
        isPuzzleActive = true;
        // 3. 퍼즐 완료 체크
        while (!isCompleted)
        {
            isCompleted = true;
            foreach (var handler in pieceHandlers)
            {
                if (!handler.isCorrectPosition)
                {
                    isCompleted = false;
                    break;
                }
            }
            yield return null;
        }
        completeImage.gameObject.SetActive(true);
        Chapter2Manager.Instance.OnPaperPuzzleDone();
        
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Mouse0));
        Ending();
    }

    void Ending()
    {
        isPuzzleActive = false;
        puzzlePanel.interactable = false;
        puzzlePanel.blocksRaycasts = false;
        puzzlePanel.alpha = 0;
        Chapter2Manager.Instance.ch2_movable = true;
    }
    public void ExittooltipOn()
    {
        completeText.gameObject.SetActive(true);
    }
}
