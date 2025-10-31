using System.Collections;
using UnityEngine;

public class NPC_FadeIn : MonoBehaviour
{
    public Transform player;        // 플레이어 Transform 참조
    public float triggerDistance = 3f; // 감지 거리
    public float fadeDuration = 2f; // 페이드인 속도

    private SpriteRenderer sr;
    private bool hasFadedIn = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0f;              // 완전 투명으로 시작
        sr.color = c;
    }

    void Update()
    {
        if (hasFadedIn) return;

        float distance = Vector2.Distance(player.position, transform.position);

        if (distance <= triggerDistance)
        {
            StartCoroutine(FadeInNPC());
            hasFadedIn = true;
        }
    }

    private IEnumerator FadeInNPC()
    {
        float elapsed = 0f;
        Color c = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration); // 0 → 1로 서서히
            sr.color = c;
            yield return null;
        }

        c.a = 1f;
        sr.color = c;
    }
}
