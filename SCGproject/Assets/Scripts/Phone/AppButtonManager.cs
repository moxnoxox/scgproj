using UnityEngine;
using UnityEngine.UI;

public class AppButtonManager : MonoBehaviour
{
    [Header("버튼")]
    public Button messageButton;
    public Button internetButton;
    public Button galleryButton;
    public Button SNSButton;

    [Header("앱 패널")]
    public GameObject messagePanel;
    public GameObject internetPanel;
    public GameObject galleryPanel;
    public GameObject SNSPanel;

    void Start()
    {
        messageButton.onClick.AddListener(() => AppManager.Instance.OpenApp(messagePanel));
        internetButton.onClick.AddListener(() => AppManager.Instance.OpenApp(internetPanel));
        galleryButton.onClick.AddListener(() => AppManager.Instance.OpenApp(galleryPanel));
        SNSButton.onClick.AddListener(() => AppManager.Instance.OpenApp(SNSPanel));
    }
}
