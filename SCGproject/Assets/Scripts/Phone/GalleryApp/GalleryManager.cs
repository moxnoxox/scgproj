using UnityEngine;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour
{
    [Header("References")]
    public Transform content;       
    public GameObject photoPrefab;   
    public GameObject popupPanel;    
    public Image largeImage;   

    [Header("Test Sprites")]
    public Sprite[] testSprites;

    void Start()
    {
        // 실행하면 testSprites에 넣은 사진들이 자동으로 뜸
        foreach (var sprite in testSprites)
        {
            AddPhoto(sprite);
        }
    }
    
    public void AddPhoto(Sprite sprite)
    {
        GameObject photoObj = Instantiate(photoPrefab, content);
        Image photoImage = photoObj.GetComponent<Image>();
        photoImage.sprite = sprite;

        Button btn = photoObj.GetComponent<Button>();
        btn.onClick.AddListener(() => ShowPopup(sprite));
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
