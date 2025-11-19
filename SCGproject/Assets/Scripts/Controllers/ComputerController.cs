using UnityEngine;

public class ComputerController : MonoBehaviour
{
    public static ComputerController Instance { get; private set; }
    public CanvasGroup canvasGroup;

    void Awake()
    {
        Instance = this;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void StartComputer()
    {
        Chapter2Manager.Instance.ch2_movable = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void OnClickMusic1()
    {
        
    }
    public void OnclickMusic2()
    {
        
    }
    public void OnclickMusic3()
    {
        
    }
    public void OnClickExit()
    {
        Chapter2Manager.Instance.ch2_movable = true;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
