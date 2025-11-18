using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RestartMenuController : MonoBehaviour
{
    public static RestartMenuController Instance;

    public CanvasGroup menu;
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetMenuVisible(false);

        // ESC 핸들러는 여기서 딱 한 번 등록
        BackInputManager.Register(OnEsc);
    }

    private void OnDestroy()
    {
        // 혹시 에디터 리셋이나 종료 시 정리
        BackInputManager.Unregister(OnEsc);
    }

    private void OnEsc()
    {
        ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    private void OpenMenu()
    {
        isOpen = true;
        SetMenuVisible(true);
        Time.timeScale = 0f;
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void CloseMenu()
    {
        isOpen = false;
        SetMenuVisible(false);
        Time.timeScale = 1f;
    }

    private void SetMenuVisible(bool visible)
    {
        if (menu == null) return;

        menu.alpha = visible ? 1f : 0f;
        menu.interactable = visible;
        menu.blocksRaycasts = visible;
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SetMenuVisible(false);
        isOpen = false;

        SceneManager.LoadScene("Chapter1");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 챕터가 바뀔 때마다 OnEsc를 항상 재등록
        BackInputManager.Unregister(OnEsc); // 혹시 중복 방지
        BackInputManager.Register(OnEsc);
        Debug.Log("[RestartMenu] Re-Registered OnEsc on Scene Loaded");
    }

}
