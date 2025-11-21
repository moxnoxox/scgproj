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

        if (handleButton != null)
        {
            var nav = handleButton.navigation;
            nav.mode = Navigation.Mode.None;
            handleButton.navigation = nav;
        }

        // 시작 시 포커스 비우기
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
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
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        BackInputManager.Register(ClosePhone);
    }

    // 폰 닫기
    public void ClosePhone()
    {
        if (!isOpen) return;
        MoveTo(hiddenPos);
        isOpen = false;
        dimPanel?.SetActive(false);

        // 포커스 비우기
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        BackInputManager.Unregister(ClosePhone);
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

    private void OnDisable()
    {
        // 씬이 바뀌거나, 오브젝트가 비활성화될 때
        // 혹시 스택에 남아 있을지 모르는 핸들러를 정리
        BackInputManager.Unregister(ClosePhone);
        isOpen = false;
    }


}
