using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonologueManager : MonoBehaviour
{
    public static MonologueManager Instance;

    public GameObject monologuePanel;
    public TextMeshProUGUI monologueText;

    private void Awake()
    {
         if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (monologuePanel != null)
            monologuePanel.SetActive(false);
    }

    /// <summary>
    /// 여러 개의 독백을 순차적으로 보여줌
    /// </summary>
    public void ShowMonologuesSequentially(List<string> lines, float showTime = 5f, float gapTime = 2f)
    {
        if (lines == null || lines.Count == 0) return;
        StartCoroutine(ShowMonologueRoutine(lines, showTime, gapTime));
    }

    private IEnumerator ShowMonologueRoutine(List<string> lines, float showTime, float gapTime)
    {
        foreach (var line in lines)
        {
            if (monologuePanel != null) monologuePanel.SetActive(true);
            if (monologueText != null) monologueText.text = line;

            yield return new WaitForSeconds(showTime);

            if (monologuePanel != null) monologuePanel.SetActive(false);

            yield return new WaitForSeconds(gapTime);
        }
        yield return new WaitForSeconds(gapTime);
        if (monologueText != null) monologueText.text = "";
        if (monologuePanel != null) monologuePanel.SetActive(false);
    }
}
