using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RestartMenuController : MonoBehaviour
{
    public static RestartMenuController Instance;

    public CanvasGroup menu;
    private bool isOpen = false;
    private bool wasInputBlocked = false; // ESC 메뉴를 열기 직전 차단 상태였는지 기록

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
        wasInputBlocked = InputBlocker.isBlocked; // 현재 차단 상태 기억
        if (wasInputBlocked)
            InputBlocker.Disable();  

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

         if (wasInputBlocked)
        {
            InputBlocker.Enable(); // 원래 차단돼 있었으면 복구
            wasInputBlocked = false;
        }
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

        // 진행 데이터/정적 핸들러 초기화
        PlayerPrefs.DeleteAll();
        BackInputManager.ClearAll();
        ChatManager.ResetStatics();
        InputBlocker.Cleanup();

        // DontDestroyOnLoad 싱글턴 정리
        if (ChatAppManager.Instance != null) Destroy(ChatAppManager.Instance.gameObject);
        if (PhoneDataManager.Instance != null) Destroy(PhoneDataManager.Instance.gameObject);
        if (RestartMenuController.Instance != null && RestartMenuController.Instance != this)
            Destroy(RestartMenuController.Instance.gameObject);
        if (GameManager_ch3.Instance != null) Destroy(GameManager_ch3.Instance.gameObject);

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
