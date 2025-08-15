using System;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    private GameObject currentApp;
    public static AppManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void OpenApp(GameObject appObject)
    {
        if (currentApp != null)
            CloseCurrentApp();

        currentApp = appObject;
        currentApp.SetActive(true);

        BackInputManager.Register(CloseCurrentApp);
    }

    public void CloseCurrentApp()
    {
        if (currentApp == null) return;

        currentApp.SetActive(false);
        currentApp = null;

        BackInputManager.Unregister(CloseCurrentApp);
    }

    public bool IsAppOpen => currentApp != null;
}
