using UnityEngine;

public class BottomBarUI : MonoBehaviour
{
    public void OnBackButtonClicked()
    {
        BackInputManager.TriggerBack();
    }

    public void OnHomeButtonClicked()
    {
        BackInputManager.ClearAll();                 
        AppManager.Instance.CloseCurrentApp();       
        if (PhonePanelController.Instance != null && PhonePanelController.Instance.IsOpen)
        {
            BackInputManager.Register(PhonePanelController.Instance.ClosePhone);
        }
    }

}
