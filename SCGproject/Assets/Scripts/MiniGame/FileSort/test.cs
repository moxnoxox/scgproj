using UnityEngine;

public class FileSortGameTest : MonoBehaviour
{
    void Start()
    {
        // 씬 실행되자마자 미니게임 띄우기
        if (FileSortGameManager.Instance != null)
        {
            FileSortGameManager.Instance.ShowGameUI();
        }
        else
        {
            Debug.LogWarning("FileSortGameManager.Instance가 없음. 씬에 FileSortGameManager 배치됐는지 확인.");
        }
    }
}

