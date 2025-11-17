using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
public class usbUI : MonoBehaviour
{
    public UnityEngine.UI.Image usb1;
    public UnityEngine.UI.Image usb2;
    public UnityEngine.UI.Image usb3;
    private static bool usb1_state = false;
    private static bool usb2_state = false;
    private static bool usb3_state = false;
    void Start()
    {
        usb1.enabled = false;
        usb2.enabled = false;
        usb3.enabled = false;
    }

    void Update()
    {
        usb1.enabled = usb1_state;
        usb2.enabled = usb2_state;
        usb3.enabled = usb3_state;
    }
    public static void usb1Active()
    {
        usb1_state = true;
    }
    public static void usb2Active()
    {
        usb2_state = true;
    }
    public static void usb3Active()
    {
        usb3_state = true;
    }
}
