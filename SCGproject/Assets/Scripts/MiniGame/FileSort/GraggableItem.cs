using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Canvas parentCanvas;            // FileSortGameCanvas
    public Transform homeParent;           // 기본은 DesktopArea
    public Vector2 rememberHomePosition;   // 초기 위치 저장
    public TextMeshProUGUI label;          // 파일명 표시용

    [HideInInspector] public FileData fileData;
    [HideInInspector] public bool isCorrectlyPlaced = false;

    RectTransform rect;
    CanvasGroup canvasGroup;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();


        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
                Debug.LogWarning($"{name}: parentCanvas를 자동으로 찾지 못했습니다.");
        }
    }

    public void SetFileData(FileData data)
    {
        fileData = data;
        if (label) label.text = $"{data.fileName}.{data.extension}";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(parentCanvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 슬롯으로 흘러들어가지 않았다면 바탕화면으로 복귀
        if (transform.parent == parentCanvas.transform)
            ReturnHome();
    }

    public void SnapTo(Transform newParent)
    {
        transform.SetParent(newParent, false);
        rect.anchoredPosition = Vector2.zero;
    }

    public void ReturnHome()
    {
        // 정답으로 놓여 있던 상태였다면 해제
        if (isCorrectlyPlaced)
        {
            FileSortGameManager.Instance.NotifyPlacementResult(true, false);
            isCorrectlyPlaced = false;
        }

        transform.SetParent(homeParent, false);
        rect.anchoredPosition = rememberHomePosition;
    }
}
