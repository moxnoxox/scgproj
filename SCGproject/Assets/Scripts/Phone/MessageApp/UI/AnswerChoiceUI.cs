using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(LayoutElement))]
public class AnswerChoiceUI : MonoBehaviour
{
    [Header("Label")]
    public TextMeshProUGUI label;          // 버튼 안의 TMP 라벨

    [Header("Width Settings")]
    public float paddingX = 10f;           // 좌우 패딩 (한쪽 기준)
    public float minWidth = 20f;
    public float maxWidth = 300f;          // 화면 폭에 맞춰 제한

    private LayoutElement le;
    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        le = GetComponent<LayoutElement>();

        if (!label)
            label = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void SetText(string text)
    {
        if (!label) return;

        label.enableWordWrapping = false;
        label.text = text;

        StartCoroutine(UpdateSizeNextFrame());
    }

    private IEnumerator UpdateSizeNextFrame()
    {
        // 한 프레임 기다림
        yield return null;

        if (!label || !le || !rt) yield break;

        float w = label.preferredWidth + paddingX * 2f;
        w = Mathf.Clamp(w, minWidth, maxWidth);

        le.preferredWidth = w;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
    }
}
