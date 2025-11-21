using UnityEngine;

public class ComputerController : MonoBehaviour
{
    public static ComputerController Instance { get; private set; }
    public CanvasGroup canvasGroup;
    public GameObject music1;
    public GameObject music2;
    public GameObject music3;
    private Vector3 targetPosition;
    void Awake()
    {
        Instance = this;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        music1.SetActive(false);
        music2.SetActive(false);
        music3.SetActive(false);
    }

    public void StartComputer()
    {
        Chapter2Manager.Instance.ch2_movable = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        targetPosition = MonologueManager.Instance.monologuePanel.transform.position;
        targetPosition.y -= 40;
        MonologueManager.Instance.monologuePanel.transform.position = targetPosition;
    }

    public void OnClickMusic1()
    {
        Chapter2Manager.Instance.OnMusicInteract1();
    }
    public void OnclickMusic2()
    {
        Chapter2Manager.Instance.OnMusicInteract2();
    }
    public void OnclickMusic3()
    {
        Chapter2Manager.Instance.OnMusicInteract3();
    }
    public void OnClickExit()
    {
        Chapter2Manager.Instance.ch2_movable = true;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        targetPosition.y += 40;
        MonologueManager.Instance.monologuePanel.transform.position = targetPosition;
    }
    void Update()
    {
        if(usbUI.isUsb1Active())
        {
            music1.SetActive(true);
        }
        if(usbUI.isUsb2Active())
        {
            music2.SetActive(true);
        }
        if(usbUI.isUsb3Active())
        {
            music3.SetActive(true);
        }
    }
}
