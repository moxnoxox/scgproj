using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ingameButton : MonoBehaviour
{
    public CameraController cameraController;
    public CanvasGroup startPage; 
    public CanvasGroup ingamePage; 

    void Start()
    {
        if (startPage != null)
        {
            startPage.alpha = 1f;
            startPage.interactable = true;
            startPage.blocksRaycasts = true;
        }

        if (ingamePage != null)
        {
            ingamePage.alpha = 0f;
            ingamePage.interactable = false;
            ingamePage.blocksRaycasts = false;
            ingamePage.gameObject.SetActive(false);
        }
    }

    public void onClickStartGame()
    {
        cameraController.gameStart = true;
        StartCoroutine(FadeOutAndIn());
        GameManager.Instance.gameStarted = true;
    }

    IEnumerator FadeOutAndIn()
    {
        float duration = 1f;
        float elapsed = 0f;
        if (startPage == null || ingamePage == null) yield break;

        // --- 페이드 아웃 ---
        // 페이드 아웃 시작 시, startPage의 상호작용을 바로 비활성화합니다.
        startPage.interactable = false;
        startPage.blocksRaycasts = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            startPage.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        
        // 페이드 아웃 완료 후, 알파값을 0으로 명확하게 설정합니다.
        startPage.alpha = 0f;
        startPage.gameObject.SetActive(false);

        // --- 페이드 인 ---
        ingamePage.gameObject.SetActive(true);
        ingamePage.alpha = 0f; // 페이드 인 시작 전 알파값 0으로 설정

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ingamePage.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        // 페이드 인 완료 후, 알파값을 1.0으로 명확하게 설정합니다.
        // 이 부분이 알파값이 0.0005에서 멈추는 문제를 해결합니다.
        ingamePage.alpha = 1f; 
        
        // 페이드 인 완료 후, ingamePage의 상호작용을 활성화합니다.
        ingamePage.interactable = true;
        ingamePage.blocksRaycasts = true;
    }
}