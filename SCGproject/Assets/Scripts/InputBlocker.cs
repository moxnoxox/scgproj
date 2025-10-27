using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;  // 🔹 이 줄 추가!

public class InputBlocker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IScrollHandler
{
    private static GameObject blocker;
    private static bool isBlocked = false;

    public static void Enable()
    {
        if (blocker == null)
        {
            blocker = new GameObject("GlobalInputBlocker");
            Canvas canvas = blocker.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // 최상단
            blocker.AddComponent<GraphicRaycaster>();
            blocker.AddComponent<Image>().color = new Color(0, 0, 0, 0); // 완전 투명
        }
        blocker.SetActive(true);
        isBlocked = true;
    }

    public static void Disable()
    {
        if (blocker != null)
            blocker.SetActive(false);
        isBlocked = false;
    }

    private void Update()
    {
        if (!isBlocked) return;
        Input.ResetInputAxes(); // 키보드, 마우스, 휠 등 입력 전부 무효화
    }

    public void OnPointerDown(PointerEventData e) { }
    public void OnPointerUp(PointerEventData e) { }
    public void OnScroll(PointerEventData e) { }
}
