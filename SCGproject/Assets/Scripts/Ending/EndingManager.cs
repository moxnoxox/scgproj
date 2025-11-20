// Assets/Scripts/Ending/EndingManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class EndingManager : MonoBehaviour
{
    public enum EntryType { Image, Text }

    [System.Serializable]
    public class EndingEntry {
        public EntryType type = EntryType.Image;

        // 공통
        public float showDuration = 2f;
        public float fadeDuration = 0.5f;
        public bool skipIfLocked;
        public string unlockKey; // PlayerPrefs 키(획득 여부 체크용)

        // Image 전용
        public string spritePath; // 예: "Illustrations/guitarbag_rotated"

        // Text 전용
        public string text;       // 표시할 문자열 (코드에 직접 작성)
    }

    [Header("Image Target (씬에 있는 더미 Image + CanvasGroup)")]
    [SerializeField] private Image targetImage;
    [SerializeField] private CanvasGroup imageGroup;

    [Header("Text Prefab (TMP + CanvasGroup 포함)")]
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Transform textParent; // UICanvas 내 원하는 위치

    [Header("Sequence")]
    [SerializeField] private List<EndingEntry> entries = new List<EndingEntry>();
    [SerializeField] private bool autoPlayOnStart = true;
    [SerializeField] private UnityEvent onFinished;

    public bool IsFinished { get; private set; }

    private void Start() {
        if (autoPlayOnStart) Play();
    }

    public void Play() {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine() {
        IsFinished = false;

        // 초기 상태
        if (imageGroup != null) imageGroup.alpha = 0f;
        if (targetImage != null) targetImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(3.5f);

        foreach (var e in entries) {
            if (e == null) continue;
            if (e.skipIfLocked && !IsUnlocked(e.unlockKey)) continue;

            switch (e.type) {
                case EntryType.Image:
                    if (targetImage == null || imageGroup == null) continue;
                    var sprite = Resources.Load<Sprite>(e.spritePath);
                    if (sprite == null) { Debug.LogWarning($"[EndingManager] Sprite not found: {e.spritePath}"); continue; }

                    targetImage.sprite = sprite;
                    //targetImage.SetNativeSize();

                    yield return Fade(imageGroup, 0f, 1f, e.fadeDuration);
                    yield return new WaitForSeconds(e.showDuration);
                    yield return Fade(imageGroup, 1f, 0f, e.fadeDuration);
                    break;

                case EntryType.Text:
                    if (textPrefab == null || textParent == null) continue;
                    var go = Instantiate(textPrefab, textParent);
                    var cg = go.GetComponent<CanvasGroup>();
                    var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (cg != null) cg.alpha = 0f;
                    if (tmp != null) tmp.text = e.text;

                    yield return Fade(cg, 0f, 1f, e.fadeDuration);
                    yield return new WaitForSeconds(e.showDuration);
                    yield return Fade(cg, 1f, 0f, e.fadeDuration);

                    Destroy(go);
                    break;
            }
        }

        IsFinished = true;
        onFinished?.Invoke();
    }

    private IEnumerator Fade(CanvasGroup g, float from, float to, float duration) {
        if (g == null) yield break;
        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        g.alpha = to;
    }

    private bool IsUnlocked(string key) {
        if (string.IsNullOrEmpty(key)) return true;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

        public void MarkFinished()
    {
        IsFinished = true;
    }

}
