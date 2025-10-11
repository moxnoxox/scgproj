using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;

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

    [Header("더미 앱 버튼들")]
    public List<Button> dummyAppButtons;

    void Start()
    {
        // 정상 앱 버튼들
        messageButton.onClick.AddListener(() => AppManager.Instance.OpenApp(messagePanel));
        internetButton.onClick.AddListener(() => AppManager.Instance.OpenApp(internetPanel));
        galleryButton.onClick.AddListener(() => AppManager.Instance.OpenApp(galleryPanel));
        SNSButton.onClick.AddListener(() => AppManager.Instance.OpenApp(SNSPanel));

        // 더미 앱 버튼들
        foreach (var btn in dummyAppButtons)
        {
            btn.onClick.AddListener(() =>
            {
                MonologueManager.Instance.ShowMonologuesSequentially(
                    new List<string> { "이 앱은 잘 쓰지 않아." },
                    3f
                );
            });
        }
    }
}
