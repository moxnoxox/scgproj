using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhonePanelController : MonoBehaviour
{
    public RectTransform phonePanel;
    public Vector2 hiddenPos;
    public Vector2 visiblePos;
    public float duration = 0.3f;
    public GameObject dimPanel;

    private bool isOpen = false;
    private Coroutine moveCoroutine;

    // 폰 열기
    public void TogglePhone()
    {
        if (isOpen) return;
        MoveTo(visiblePos);
        isOpen = true;

        // 배경 어둡게
        if (dimPanel != null)
            dimPanel.SetActive(true);

        // esc 핸들러 등록
        BackInputManager.Register(ClosePhone);  
    }

    // 폰 닫기
    public void ClosePhone()
    {
        if (!isOpen) return;
        MoveTo(hiddenPos);
        isOpen = false;

        // 배경 어둡게 한 효과 해제
        if (dimPanel != null)
        dimPanel.SetActive(false);

        // esc 핸들러 제거
        BackInputManager.Unregister(ClosePhone);
    }
    
    private void MoveTo(Vector2 target)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(SlideTo(target));
    }

    IEnumerator SlideTo(Vector2 target)
    {
        Vector2 start = phonePanel.anchoredPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            phonePanel.anchoredPosition = Vector2.Lerp(start, target, time / duration);
            yield return null;
        }

        phonePanel.anchoredPosition = target;
    }

}
