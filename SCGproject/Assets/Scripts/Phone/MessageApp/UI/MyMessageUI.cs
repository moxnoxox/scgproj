using TMPro;
using UnityEngine;

public class MyMessageUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI timeText;
    public RectTransform bubble;

    [Header("Layout Settings")]
    [SerializeField] private float maxWidth = 600f;
    [SerializeField] private float bubblePaddingX = 40f;
    [SerializeField] private float bubblePaddingY = 30f;
    [SerializeField] private float spacingBubbleToTime = 6f;
    [SerializeField] private float rightMargin = 10f;  

    public void Setup(string message, string time = "", bool autoTime = true, bool showTime = true)
    {
        // 시간 결정 
        string finalTime = autoTime
            ? FindObjectOfType<GameClock>().GetTimeString()
            : time;

        // 시간 적용 
        timeText.gameObject.SetActive(showTime);
        if (showTime) timeText.text = finalTime;

        // 텍스트 적용
        messageText.text = message;
        messageText.enableWordWrapping = true;
        messageText.ForceMeshUpdate();

        // 메시지 크기
        Vector2 preferred = messageText.GetPreferredValues(message, maxWidth, Mathf.Infinity);
        float width = Mathf.Min(preferred.x, maxWidth);
        float height = preferred.y;

        // 말풍선 크기
        messageText.rectTransform.sizeDelta = new Vector2(width, height);
        bubble.sizeDelta = new Vector2(width + bubblePaddingX, height + bubblePaddingY);

        // 내부 배치 
        LayoutElements(width, height, showTime);

        // MyMessage 전체 프리팹 폭을 Content와 동일하게
        RectTransform self = GetComponent<RectTransform>();
        float fullWidth = ((RectTransform)self.parent).rect.width;
        self.sizeDelta = new Vector2(fullWidth, self.sizeDelta.y);
    }

    private void LayoutElements(float msgWidth, float msgHeight, bool showTime)
    {
        // 1. 말풍선
        bubble.anchorMin = new Vector2(1, 1);
        bubble.anchorMax = new Vector2(1, 1);
        bubble.pivot = new Vector2(1, 1);
        bubble.anchoredPosition = new Vector2(-rightMargin, 0f);

        // 2. 메시지 텍스트 
        messageText.rectTransform.pivot = new Vector2(0, 1);
        messageText.rectTransform.anchorMin = new Vector2(0, 1);
        messageText.rectTransform.anchorMax = new Vector2(0, 1);
        messageText.rectTransform.anchoredPosition = new Vector2(
            bubblePaddingX * 0.5f,
            -bubblePaddingY * 0.5f
        );
        messageText.alignment = TMPro.TextAlignmentOptions.TopLeft;

        // 3. 시간 
        if (showTime)
        {
            timeText.rectTransform.anchorMin = new Vector2(1, 1);
            timeText.rectTransform.anchorMax = new Vector2(1, 1);
            timeText.rectTransform.pivot = new Vector2(1, 1);
            timeText.rectTransform.anchoredPosition = new Vector2(
                bubble.anchoredPosition.x - bubble.sizeDelta.x - 10f,
                bubble.anchoredPosition.y - msgHeight
            );
        }

        // 4. 프리팹 전체 높이 갱신 
        float totalHeight = bubble.sizeDelta.y;
        if (showTime)
            totalHeight += spacingBubbleToTime;

        GetComponent<RectTransform>().sizeDelta = new Vector2(
            GetComponent<RectTransform>().sizeDelta.x,
            totalHeight
        );
    }



    /// <summary>
    /// ChatManager에서 같은 시간대 직전 메시지의 시간 숨길 때 호출
    /// </summary>
    public void SetTimeVisible(bool visible)
    {
        timeText.gameObject.SetActive(visible);
    }
}
