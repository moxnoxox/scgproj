using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class PaperpuzzleController : MonoBehaviour
{
    public static PaperpuzzleController Instance;
    public GameObject[] pieces;
    public PaperHandler[] pieceHandlers;
    public bool isPuzzleActive;
    public CanvasGroup puzzlePanel;
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
        puzzlePanel.alpha = 0;
        puzzlePanel.interactable = false;
        puzzlePanel.blocksRaycasts = false;
        //디버그용
    }
    public void StartPuzzle()
    {
        StartCoroutine(PuzzleRoutine());
    }

    IEnumerator PuzzleRoutine()
    {
        
        puzzlePanel.interactable = true;
        puzzlePanel.blocksRaycasts = true;
        puzzlePanel.alpha = 1;

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
        // 퍼즐 완료 처리
        isPuzzleActive = false;
        puzzlePanel.interactable = false;
        puzzlePanel.blocksRaycasts = false;
        puzzlePanel.alpha = 0;
        Chapter2Manager.Instance.OnPaperPuzzleDone();
        Chapter2Manager.Instance.movable = true;
        yield return null;
    }
}
