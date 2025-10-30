using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonologueManager : MonoBehaviour
{
    public static MonologueManager Instance;

    public GameObject monologuePanel;
    public TextMeshProUGUI monologueText;
    public TextMeshProUGUI announcementText;
   

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    /// <summary>
    /// 여러 개의 독백을 순차적으로 보여줌
    /// </summary>
    public void ShowMonologuesSequentially(List<string> lines, float showTime = 5f)
    {
        if (lines == null || lines.Count == 0) return;
        StartCoroutine(ShowMonologueRoutine(lines, showTime));
    }

    public IEnumerator ShowMonologueRoutine(List<string> lines, float showTime)
    {
        if (lines == null || lines.Count == 0)
        {
            monologueText.gameObject.SetActive(false);
            yield break;
        }
        monologueText.gameObject.SetActive(true);
        foreach (var line in lines)
        {
            monologueText.text = line;
            yield return new WaitForSeconds(showTime);
        }
        monologueText.gameObject.SetActive(false);
    }
    public void ShowAnnouncement(List<string> messages, float duration = 3f)
    {
        if (messages == null || messages.Count == 0 || announcementText == null) return;
        StartCoroutine(ShowAnnouncementRoutine(messages, duration));
    }
    // 여러 줄도 가능하게
    private IEnumerator ShowAnnouncementRoutine(List<string> messages, float duration)
    {
        if (messages == null || messages.Count == 0)
        {
            announcementText.gameObject.SetActive(false);
            yield break;
        }
        announcementText.gameObject.SetActive(true);

        foreach (var line in messages)
        {
            announcementText.text = line;
            yield return new WaitForSeconds(duration);
        }

        announcementText.gameObject.SetActive(false);
    }

}
