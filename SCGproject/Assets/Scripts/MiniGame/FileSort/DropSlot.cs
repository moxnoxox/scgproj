using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public FileCategory acceptsCategory;
    public Transform contentRoot;  // 아이콘이 착지할 자식(없으면 자기 transform)

    void Reset()
    {
        if (contentRoot == null) contentRoot = transform;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        var item = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (item == null) return;

        // 정답 체크
        bool correct = (item.category == acceptsCategory);

        if (correct)
        {
            item.SnapTo(contentRoot);
            if (!item.isCorrectlyPlaced)
            {
                item.isCorrectlyPlaced = true;
                FileSortGameManager.Instance.NotifyPlacementChanged(item, true);
            }
        }
        else
        {
            // 오답이면 원위치
            item.ReturnHome();
        }
    }
}
