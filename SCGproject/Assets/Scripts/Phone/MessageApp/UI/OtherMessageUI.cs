using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OtherMessageUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image profileImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI timeText;
    public RectTransform bubble; 

    [Header("Layout Settings")]
    [SerializeField] private float maxWidth = 600f;
    [SerializeField] private float bubblePaddingX = 40f;
    [SerializeField] private float bubblePaddingY = 30f;
    [SerializeField] private float profileOffsetX = 20f;
    [SerializeField] private float profileSize = 80f;
    [SerializeField] private float spacingNameToBubble = 15f;   // 이름 → 말풍선 간격
    [SerializeField] private float spacingBetweenBubbles = 10f; // 연속 말풍선 간격
    [SerializeField] private float spacingBubbleToTime = 6f;

    /// <summary>
    /// OtherMessage 세팅
    /// </summary>
    public void Setup(string senderName, Sprite profile, string message, string time = "",
                      bool showProfile = true, bool showName = true, bool showTime = true, bool autoTime = true)
    {
        // 시간 결정
        string finalTime = autoTime 
            ? FindObjectOfType<GameClock>().GetTimeString() 
            : time;

        // UI 표시 여부 
        profileImage.gameObject.SetActive(showProfile);
        nameText.gameObject.SetActive(showName);
        timeText.gameObject.SetActive(showTime);

        // 내용 적용
        if (showProfile)
        {
            profileImage.sprite = profile;
            profileImage.rectTransform.sizeDelta = new Vector2(profileSize, profileSize);
        }
        if (showName)nameText.text = senderName;
        if (showTime) timeText.text = finalTime;

        // 메시지 줄바꿈 허용
        messageText.text = message;
        messageText.enableWordWrapping = true;
        messageText.ForceMeshUpdate();

        // 메세지 크기 
        Vector2 preferred = messageText.GetPreferredValues(message, maxWidth, Mathf.Infinity);
        float width = Mathf.Min(preferred.x, maxWidth);
        float height = preferred.y;


        // 말풍선 크기
        messageText.rectTransform.sizeDelta = new Vector2(width, height);
        bubble.sizeDelta = new Vector2(width + bubblePaddingX, height + bubblePaddingY);

        // 내부 배치 
        LayoutElements(width, height, showProfile, showName, showTime);

        // OtherMessage 전체 프리팹 폭을 Content와 동일하게
        RectTransform self = GetComponent<RectTransform>();
        float fullWidth = ((RectTransform)self.parent).rect.width;
        self.sizeDelta = new Vector2(fullWidth, self.sizeDelta.y);
    }

    private void LayoutElements(float msgWidth, float msgHeight, bool showProfile, bool showName, bool showTime)
    {
        // 1. 프로필 위치
        if (showProfile)
        {
            profileImage.rectTransform.anchorMin = new Vector2(0, 1);
            profileImage.rectTransform.anchorMax = new Vector2(0, 1);
            profileImage.rectTransform.pivot = new Vector2(0, 1);
            profileImage.rectTransform.anchoredPosition = new Vector2(profileOffsetX, 0);
        }

        // 2. 이름 위치
        if (showName)
        {
            nameText.rectTransform.anchorMin = new Vector2(0, 1);
            nameText.rectTransform.anchorMax = new Vector2(0, 1);
            nameText.rectTransform.pivot = new Vector2(0, 1);

            float nameX = profileOffsetX + profileSize;
            nameText.rectTransform.anchoredPosition = new Vector2(nameX, 0);
        }

        // 3. 말풍선 
        bubble.anchorMin = new Vector2(0, 1);
        bubble.anchorMax = new Vector2(0, 1);
        bubble.pivot = new Vector2(0, 1);

        float bubbleX = profileOffsetX + profileSize + 10f;
        float bubbleY = 0f;
        if (showName)
            bubbleY -= nameText.preferredHeight + spacingNameToBubble;
        else
            bubbleY -= spacingBetweenBubbles; // 연속 메시지일 때 작은 간격

        bubble.anchoredPosition = new Vector2(bubbleX, bubbleY);

        // 4. 메시지 텍스트
        messageText.rectTransform.pivot = new Vector2(0, 1);
        messageText.rectTransform.anchorMin = new Vector2(0, 1);
        messageText.rectTransform.anchorMax = new Vector2(0, 1);
        messageText.rectTransform.anchoredPosition = new Vector2(
            bubblePaddingX * 0.5f,
            -bubblePaddingY * 0.5f
        );

        // 5. 시간 
        if (showTime)
        {
            timeText.rectTransform.anchorMin = new Vector2(0, 1);
            timeText.rectTransform.anchorMax = new Vector2(0, 1);
            timeText.rectTransform.pivot = new Vector2(0, 1);
            timeText.rectTransform.anchoredPosition = new Vector2(
                bubble.anchoredPosition.x + bubble.sizeDelta.x + 10f,
                bubble.anchoredPosition.y - msgHeight
            );
        }

        // 6. 프리팹 전체 높이 갱신 (VerticalLayoutGroup용)
        float totalHeight = bubble.sizeDelta.y;
        if (showName)
            totalHeight += nameText.preferredHeight + spacingNameToBubble;
        else
            totalHeight += spacingBetweenBubbles;

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
