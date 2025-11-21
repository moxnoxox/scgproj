using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;  // 🔹 이 줄 추가!

public class InputBlocker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IScrollHandler
{
    private static GameObject blocker;
    public static bool isBlocked = false;

    public static void Enable()
    {
        if (blocker == null)
        {
            blocker = new GameObject("GlobalInputBlocker");

            // 화면 최상단 캔버스
            var canvas = blocker.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            blocker.AddComponent<GraphicRaycaster>();

            // ✅ 반드시 자기 자신에 컴포넌트를 붙여서 Update()가 돌게 한다
            blocker.AddComponent<InputBlocker>();

            // ✅ 화면 전체를 덮도록 RectTransform 설정
            var rt = blocker.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 투명 이미지(레이캐스트 타겟 기본값 = true)
            var img = blocker.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
        }

        blocker.SetActive(true);
        isBlocked = true;

        // 이벤트 시스템이 없으면 클릭 차단이 안 됨
        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
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
    }

    public static void Cleanup()
    {
        Disable();
        if (blocker != null)
        {
            Object.Destroy(blocker);
            blocker = null;
        }
    }


    public void OnPointerDown(PointerEventData e) { }
    public void OnPointerUp(PointerEventData e) { }
    public void OnScroll(PointerEventData e) { }
}
