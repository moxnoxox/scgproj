using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PhonePanelDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("필수 참조")]
    public RectTransform phonePanel;                
    public PhonePanelController controller;       

    [Header("동작 옵션")]
    public float dismissDistance = 180f;             // 얼마나 드래그 하면 닫힐지
    public bool onlyWhenOpen = true;                 // 열렸을 때만 드래그 허용

    // 내부 상태
    private Vector2 startPanelPos;                   // 드래그 시작 시 패널 위치
    private Vector2 pointerStartLocal;               // 드래그 시작 마우스 포인터 위치
    private bool dragging;                           // 드래그 중인지
    private Coroutine snapBackCo;                    // 원위치 돌아가는 코루틴

    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (onlyWhenOpen && !controller.IsOpen) return;

        var parent = phonePanel.parent as RectTransform;
        if (parent == null) return;

        dragging = true;
        startPanelPos = phonePanel.anchoredPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position, eventData.pressEventCamera, out pointerStartLocal
        );
    }

    // 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        var parent = phonePanel.parent as RectTransform;
        if (parent == null) return;

        Vector2 pointerNowLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position, eventData.pressEventCamera, out pointerNowLocal
        );

        Vector2 delta = pointerNowLocal - pointerStartLocal;

        // y 좌표 아래로만 움직이게
        float newY = startPanelPos.y + Mathf.Min(0f, delta.y);
        phonePanel.anchoredPosition = new Vector2(startPanelPos.x, newY);
    }

    // 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;

        // 얼마나 드래그 했는지 확인
        float pulled = startPanelPos.y - phonePanel.anchoredPosition.y;
        if (pulled >= dismissDistance)
        {
	        controller.ClosePhone();
        }
        else
        {
            // 임계치 미만이면 원위치로 스냅백
            if (snapBackCo != null) StopCoroutine(snapBackCo);
            snapBackCo = StartCoroutine(SnapBack());
        }
    }

    private IEnumerator SnapBack()
    {
        Vector2 from = phonePanel.anchoredPosition;
        Vector2 to = controller.visiblePos; 
        float dur = controller.duration;   
        float t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            phonePanel.anchoredPosition = Vector2.Lerp(from, to, k);
            yield return null;
        }
        phonePanel.anchoredPosition = to;
    }
}