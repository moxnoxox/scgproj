using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OtherMessageUI : MonoBehaviour
{
    [Header("공통 UI")]
    public Image profileImage;
    public TextMeshProUGUI nameText;

    [Header("텍스트 메시지용")]
    public GameObject otherBubble;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI timeText_text;
    public RectTransform bubble; 

    [Header("이미지 메시지용")]
    public GameObject imageObject;           
    public Image imageComponent;             
    public TextMeshProUGUI timeText_image;   

    [Header("레이아웃 설정")]
    [SerializeField] private float maxWidth = 300f;
    [SerializeField] private float maxImageHeight = 300f;
    [SerializeField] private float bubblePaddingX = 40f;
    [SerializeField] private float bubblePaddingY = 30f;
    [SerializeField] private float profileOffsetX = 20f;
    [SerializeField] private float profileSize = 80f;
    [SerializeField] private float spacingNameToBubble = 15f;
    [SerializeField] private float spacingBetweenBubbles = 10f;
    [SerializeField] private float spacingBubbleToTime = 6f;

    private RectTransform self;

    private void Awake()
    {
        self = GetComponent<RectTransform>();
    }

    // --- 텍스트 메시지 ---
    public void SetupText(string senderName, Sprite profile, string message, string time = "",
                          bool showProfile = true, bool showName = true, bool showTime = true, bool autoTime = true)
    {
        SetActiveMode(isImage: false);

        string finalTime = autoTime ? FindObjectOfType<GameClock>().GetTimeString() : time;

        // 프로필/이름 표시
        profileImage.gameObject.SetActive(showProfile);
        nameText.gameObject.SetActive(showName);
        timeText_text.gameObject.SetActive(showTime);

        if (showProfile)
        {
            profileImage.sprite = profile;
            profileImage.rectTransform.sizeDelta = new Vector2(profileSize, profileSize);
            profileImage.rectTransform.anchorMin = profileImage.rectTransform.anchorMax = new Vector2(0, 1);
            profileImage.rectTransform.pivot = new Vector2(0, 1);
            profileImage.rectTransform.anchoredPosition = new Vector2(profileOffsetX, 0);
        }

        if (showName)
        {
            nameText.text = senderName;
            nameText.rectTransform.anchorMin = nameText.rectTransform.anchorMax = new Vector2(0, 1);
            nameText.rectTransform.pivot = new Vector2(0, 1);
            nameText.rectTransform.anchoredPosition = new Vector2(profileOffsetX + profileSize, 0);
        }

        // 텍스트 계산
        messageText.text = message;
        messageText.enableWordWrapping = true;
        messageText.ForceMeshUpdate();

        Vector2 preferred = messageText.GetPreferredValues(message, maxWidth, Mathf.Infinity);
        float width = Mathf.Min(preferred.x, maxWidth);
        float height = preferred.y;

        // 말풍선 크기
        messageText.rectTransform.sizeDelta = new Vector2(width, height);
        bubble.sizeDelta = new Vector2(width + bubblePaddingX, height + bubblePaddingY);

        // 말풍선 위치
        float bubbleX = profileOffsetX + profileSize + 10f;
        float bubbleY = showName ? -(nameText.preferredHeight + spacingNameToBubble) : -spacingBetweenBubbles;
        bubble.anchorMin = bubble.anchorMax = new Vector2(0, 1);
        bubble.pivot = new Vector2(0, 1);
        bubble.anchoredPosition = new Vector2(bubbleX, bubbleY);

        // 메시지 텍스트 위치
        messageText.rectTransform.anchorMin = messageText.rectTransform.anchorMax = new Vector2(0, 1);
        messageText.rectTransform.pivot = new Vector2(0, 1);
        messageText.rectTransform.anchoredPosition = new Vector2(bubblePaddingX * 0.5f, -bubblePaddingY * 0.5f);

        // 시간 위치
        if (showTime)
        {
            timeText_text.rectTransform.anchorMin = timeText_text.rectTransform.anchorMax = new Vector2(0, 1);
            timeText_text.rectTransform.pivot = new Vector2(0, 1);
            timeText_text.rectTransform.anchoredPosition = new Vector2(
                bubble.anchoredPosition.x + bubble.sizeDelta.x + 10f,
                bubble.anchoredPosition.y - msgHeight(bubble) + 8f
            );
            timeText_text.text = finalTime;
        }

        UpdateTotalHeight(showName, nameText.preferredHeight, bubble.sizeDelta.y);
    }

    // --- 이미지 메시지 ---
    public void SetupImage(string senderName, Sprite profile, string imagePath, string time = "",
                           bool showProfile = true, bool showName = true, bool showTime = true, bool autoTime = true)
    {
        SetActiveMode(isImage: true);

        string finalTime = autoTime ? FindObjectOfType<GameClock>().GetTimeString() : time;

        // 프로필/이름 위치는 텍스트 버전과 동일
        if (showProfile)
        {
            profileImage.sprite = profile;
            profileImage.rectTransform.sizeDelta = new Vector2(profileSize, profileSize);
            profileImage.rectTransform.anchorMin = profileImage.rectTransform.anchorMax = new Vector2(0, 1);
            profileImage.rectTransform.pivot = new Vector2(0, 1);
            profileImage.rectTransform.anchoredPosition = new Vector2(profileOffsetX, 0);
        }

        if (showName)
        {
            nameText.text = senderName;
            nameText.rectTransform.anchorMin = nameText.rectTransform.anchorMax = new Vector2(0, 1);
            nameText.rectTransform.pivot = new Vector2(0, 1);
            nameText.rectTransform.anchoredPosition = new Vector2(profileOffsetX + profileSize, 0);
        }

        // 이미지 로드
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite == null)
        {
            Debug.LogWarning($"❌ 이미지 로드 실패: {imagePath}");
            return;
        }

        imageComponent.sprite = sprite;
        // 🔹 비율 유지
        float ratio = sprite.rect.height / sprite.rect.width;

        // 🔹 기준 폭을 무조건 maxWidth로 두고 비율에 맞게 계산
        float width = maxWidth;
        float height = width * ratio;

        // 🔹 세로 제한
        if (height > maxImageHeight)
        {
            height = maxImageHeight;
            width = height / ratio;
        }

        imageComponent.rectTransform.sizeDelta = new Vector2(width, height);
        imageObject.GetComponent<RectTransform>().sizeDelta = imageComponent.rectTransform.sizeDelta;

        // 이미지 위치
        RectTransform imgRect = imageComponent.rectTransform;
        imgRect.anchorMin = imgRect.anchorMax = new Vector2(0, 1);
        imgRect.pivot = new Vector2(0, 1);
        float imgX = profileOffsetX + profileSize + 10f;
        float imgY = showName ? -(nameText.preferredHeight + spacingNameToBubble) : -spacingBetweenBubbles;
        imgRect.anchoredPosition = new Vector2(imgX, imgY);

        // 시간 위치
        if (showTime)
        {
            timeText_image.rectTransform.anchorMin = timeText_image.rectTransform.anchorMax = new Vector2(0, 1);
            timeText_image.rectTransform.pivot = new Vector2(0, 1);
            timeText_image.rectTransform.anchoredPosition = new Vector2(
                imgRect.anchoredPosition.x + width + 10f,
                imgRect.anchoredPosition.y - height + 18f
            );
            timeText_image.text = finalTime;
        }

        UpdateTotalHeight(showName, nameText.preferredHeight, height);
    }

    private void SetActiveMode(bool isImage)
    {
        otherBubble.SetActive(!isImage);
        messageText.gameObject.SetActive(!isImage);
        timeText_text.gameObject.SetActive(!isImage);

        imageObject.SetActive(isImage);
        imageComponent.gameObject.SetActive(isImage);
        timeText_image.gameObject.SetActive(isImage);
    }

    private void UpdateTotalHeight(bool showName, float nameHeight, float contentHeight)
    {
        float totalHeight = contentHeight;
        if (showName) totalHeight += nameHeight + spacingNameToBubble;
        else totalHeight += spacingBetweenBubbles;

        self.sizeDelta = new Vector2(self.sizeDelta.x, totalHeight);
    }

    private float msgHeight(RectTransform bubble)
    {
        return bubble.sizeDelta.y - bubblePaddingY * 0.5f;
    }

    public void SetTimeVisible(bool visible)
    {
        timeText_text.gameObject.SetActive(visible);
        timeText_image.gameObject.SetActive(visible);
    }
}
