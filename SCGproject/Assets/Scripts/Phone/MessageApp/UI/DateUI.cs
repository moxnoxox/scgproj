using UnityEngine;

public class DateDividerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform capsuleBG;   // 캡슐 배경 (Image)

    [Header("Layout Settings")]
    [SerializeField] private float capsuleWidth = 220f;
    [SerializeField] private float capsuleHeight = 45f;
    [SerializeField] private float topBottomPadding = 5f;

    public void Setup()
    {
        capsuleBG.sizeDelta = new Vector2(capsuleWidth, capsuleHeight);

        capsuleBG.anchorMin = new Vector2(0.5f, 0.5f);
        capsuleBG.anchorMax = new Vector2(0.5f, 0.5f);
        capsuleBG.pivot     = new Vector2(0.5f, 0.5f);
        capsuleBG.anchoredPosition = Vector2.zero;

        RectTransform self = GetComponent<RectTransform>();
        float fullWidth = ((RectTransform)self.parent).rect.width;
        float totalHeight = capsuleHeight + topBottomPadding * 2;
        self.sizeDelta = new Vector2(fullWidth, totalHeight);
    }
}
