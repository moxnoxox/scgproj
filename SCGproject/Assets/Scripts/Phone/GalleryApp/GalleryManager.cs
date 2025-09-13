using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour
{
    [Header("References")]
    public Transform content;       
    public GameObject photoPrefab;   
    public GameObject popupPanel;    
    public Image largeImage;   

    [Header("Test Sprites")]
    public Sprite[] testSprites;

    [Header("잠금 상태")]
    public List<bool> lockedStates = new List<bool>(); // 사진별 잠금 여부

    void Start()
    {
        // testSprites 개수만큼 잠금 상태 초기화
        if (lockedStates.Count < testSprites.Length)
        {
            // 부족하면 true(잠금 상태)로 채움
            for (int i = lockedStates.Count; i < testSprites.Length; i++)
                lockedStates.Add(true);
        }

        for (int i = 0; i < testSprites.Length; i++)
        {
            AddPhoto(testSprites[i], i);
        }
    }
    
    public void AddPhoto(Sprite sprite, int index)
    {
        GameObject photoObj = Instantiate(photoPrefab, content);
        Image photoImage = photoObj.GetComponent<Image>();
        photoImage.sprite = sprite;

        Button btn = photoObj.GetComponent<Button>();
        btn.onClick.AddListener(() => OnPhotoClicked(sprite, index));
    }

    private void OnPhotoClicked(Sprite sprite, int index)
    {
        if (lockedStates[index])
        {
            // 🔒 잠겨 있으면 팝업 대신 독백 출력
            MonologueManager.Instance.ShowMonologuesSequentially(
                new List<string> { "\'잠금 해제 시 열람 가능\'" },
                2f, 0f
            );
        }
        else
        {
            // 🔓 열려 있으면 원래대로 팝업 실행
            ShowPopup(sprite);
        }
    }

    private void ShowPopup(Sprite sprite)
    {
        largeImage.sprite = sprite;
        popupPanel.SetActive(true);

        BackInputManager.Register(ClosePopup);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);

        BackInputManager.Unregister(ClosePopup);
    }

    void OnDisable()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}
