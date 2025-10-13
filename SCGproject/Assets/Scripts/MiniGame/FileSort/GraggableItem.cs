using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public FileCategory category;
    public Canvas parentCanvas;            // FileSortGameCanvas 참조
    [HideInInspector] public Transform homeParent;
    [HideInInspector] public bool isCorrectlyPlaced = false;

    CanvasGroup canvasGroup;
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        homeParent = transform.parent;
        transform.SetParent(parentCanvas.transform, true);
        canvasGroup.blocksRaycasts = false; // 드랍받도록
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 드롭 실패(슬롯에 못 내려앉음)면 복귀
        if (transform.parent == parentCanvas.transform)
        {
            ReturnHome();
        }
    }

    public void SnapTo(Transform newParent)
    {
        transform.SetParent(newParent, false);
        rect.anchoredPosition = Vector2.zero;
    }

    public void ReturnHome()
    {
        // 이미 정답으로 놓였던 걸 빼온 경우, 상태 갱신
        if (isCorrectlyPlaced)
        {
            isCorrectlyPlaced = false;
            FileSortGameManager.Instance.NotifyPlacementChanged(this, false);
        }
        SnapTo(homeParent);
    }
}
