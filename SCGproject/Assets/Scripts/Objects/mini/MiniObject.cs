using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniObjectBounceOnce : MonoBehaviour, IInteractable
{
    [TextArea]
    public string message = "무언가가 튀어 올랐다.";
    public int powerChange = -5;

    private bool hasInteracted = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPos;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.enabled = false; // 처음엔 안 보이게
        startPos = transform.position;
    }

    public void Interact(PlayerMove player)
    {
        if (hasInteracted) return;
        hasInteracted = true;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        StartCoroutine(BounceAndDisappear(player));
    }

    private IEnumerator BounceAndDisappear(PlayerMove player)
    {
        float upHeight = 0.75f;   // 위로 튀어오르는 높이
        float upTime = 0.3f;     // 올라가는 시간
        float downTime = 0.6f;   // 내려가는 시간
        float fadeTime = 0.4f;   // 사라지는 시간

        // 위로 부드럽게 올라가기
        Vector3 topPos = startPos + Vector3.up * upHeight;
        for (float t = 0; t < upTime; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(startPos, topPos, t / upTime);
            yield return null;
        }

        // 🔹 약간의 대기 후 대사 출력
        yield return new WaitForSeconds(0.3f);

        if (MonologueManager.Instance != null)
        {
            List<string> lines = new List<string> { message };
            MonologueManager.Instance.ShowMonologuesSequentially(lines, 2f);
        }

        // 에너지 증감 (대사와 동시에)
        if (player.playerPower != null)
        {
            if (powerChange < 0)
                player.playerPower.DecreasePower(Mathf.Abs(powerChange));
            else
                player.playerPower.IncreasePower(powerChange);
        }

        // 아래로 서서히 내려가며 페이드아웃
        Vector3 endPos = startPos + Vector3.down * 0.8f;
        Color c = spriteRenderer.color;
        for (float t = 0; t < downTime; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(topPos, endPos, t / downTime);
            c.a = Mathf.Lerp(1f, 0f, t / (downTime + fadeTime));
            spriteRenderer.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }
}
