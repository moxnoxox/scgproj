using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public FileCategory acceptsCategory;  // Photo / Docs / Music / Etc / Trash
    public Transform contentRoot;

    void Reset()
    {
        if (!contentRoot) contentRoot = transform;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        var item = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (item == null || item.fileData == null) return;

        bool wasCorrect = item.isCorrectlyPlaced;
        bool nowCorrect = IsCorrectForThisSlot(item.fileData);

        if (nowCorrect)
        {
            // 정답: 개수 반영 후 즉시 삭제
            if (!item.isCorrectlyPlaced)
            {
                item.isCorrectlyPlaced = true;
                FileSortGameManager.Instance.NotifyPlacementResult(wasCorrect, true);
            }

            // 사라지게
            Destroy(item.gameObject);
            // Debug
            Debug.Log($"[DropSlot] CORRECT → {item.fileData.fileName}.{item.fileData.extension} -> {acceptsCategory}");
        }
        else
        {
            // 오답: 패널티 후 원위치
            Debug.Log($"[DropSlot] WRONG → {item.fileData.fileName}.{item.fileData.extension} -> {acceptsCategory}  (-5s)");
            FileSortGameManager.Instance.ApplyPenalty(5f);
            item.ReturnHome();
        }
    }

    bool IsCorrectForThisSlot(FileData data)
    {
        // 악성/중복/위장 파일은 Trash만 정답
        if (data.isMalicious) return acceptsCategory == FileCategory.Trash;

        // 정상 파일은 지정된 카테고리만 정답
        return acceptsCategory == data.correctCategory;
    }
}
