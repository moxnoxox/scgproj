using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhonePanelController : MonoBehaviour
{
    public static PhonePanelController Instance { get; private set; }

    public RectTransform phonePanel;
    public Vector2 hiddenPos;
    public Vector2 visiblePos;
    public float duration = 0.3f;
    public GameObject dimPanel;
    public Button handleButton;
    private bool enabled;
    private int SceneNum;

    private bool isOpen = false;
    private Coroutine moveCoroutine;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Chapter1")
        {
            SceneNum = 1;
            enabled = GameManager.Instance.phoneOpenEnable;
        }
        else if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Chapter2")
        {
            SceneNum = 2;
            enabled = true;//추후 조정
        }
    }

    void Update()
    {
        if(SceneNum == 1)
        {
            enabled = GameManager.Instance.phoneOpenEnable;
        }
        else if(SceneNum == 2)
        {
            enabled = true;//추후 조정
        }
        if (enabled)
        {
            handleButton.interactable = true;
        }
        else
        {
            handleButton.interactable = false;
            if (isOpen)
                ClosePhone();
        }
    }

    // 폰 열기
    public void TogglePhone()
    {
        if (isOpen) return;
        MoveTo(visiblePos);
        isOpen = true;

        // 배경 어둡게
        if (dimPanel != null)
            dimPanel.SetActive(true);

        // esc 핸들러 등록
        BackInputManager.Register(ClosePhone);
        EventSystem.current.SetSelectedGameObject(null);
    }

    // 폰 닫기
    public void ClosePhone()
    {
        if (!isOpen) return;
        MoveTo(hiddenPos);
        isOpen = false;

        // 배경 어둡게 한 효과 해제
        if (dimPanel != null)
            dimPanel.SetActive(false);

        // esc 핸들러 제거
        BackInputManager.Unregister(ClosePhone);
        EventSystem.current.SetSelectedGameObject(null);
    }
    
    private void MoveTo(Vector2 target)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(SlideTo(target));
    }

    IEnumerator SlideTo(Vector2 target)
    {
        Vector2 start = phonePanel.anchoredPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            phonePanel.anchoredPosition = Vector2.Lerp(start, target, time / duration);
            yield return null;
        }

        phonePanel.anchoredPosition = target;
    }

}
