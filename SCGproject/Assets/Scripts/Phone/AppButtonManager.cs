using UnityEngine;
using UnityEngine.UI;

public class AppButtonManager : MonoBehaviour
{
    [Header("버튼")]
    public Button messageButton;
    public Button internetButton;
    public Button galleryButton;

    [Header("앱 패널")]
    public GameObject messagePanel;
    public GameObject internetPanel;
    public GameObject galleryPanel;

    void Start()
    {
        messageButton.onClick.AddListener(() => AppManager.Instance.OpenApp(messagePanel));
        internetButton.onClick.AddListener(() => AppManager.Instance.OpenApp(internetPanel));
        galleryButton.onClick.AddListener(() => AppManager.Instance.OpenApp(galleryPanel));
    }
}
