using UnityEngine;
using UnityEngine.UI;

public class NewsManager : MonoBehaviour
{
    [SerializeField] private GameObject buttons;        // 뉴스 목록 버튼들 그룹
    [SerializeField] private GameObject scrollView;     // 뉴스 상세 스크롤 뷰
    [SerializeField] private Button[] newsButtons;      // 버튼 5개
    [SerializeField] private GameObject[] newsPanels;   // ScrollView 안의 News1~5

    void Start()
    {
        scrollView.SetActive(false);
        foreach (var panel in newsPanels)
            panel.SetActive(false);

        for (int i = 0; i < newsButtons.Length; i++)
        {
            int index = i;
            newsButtons[i].onClick.AddListener(() => OpenNews(index));
        }
    }

    void OpenNews(int index)
    {
        buttons.SetActive(false);
        scrollView.SetActive(true);

        // 선택한 뉴스만 켜기
        for (int i = 0; i < newsPanels.Length; i++)
            newsPanels[i].SetActive(i == index);

        RectTransform newsRect = newsPanels[index].GetComponent<RectTransform>();
        RectTransform contentRect = newsRect.transform.parent.GetComponent<RectTransform>();

        contentRect.sizeDelta = new Vector2(
            contentRect.sizeDelta.x,
            newsRect.sizeDelta.y
        );

        System.Action closeHandler = null;
        closeHandler = () =>
        {
            newsPanels[index].SetActive(false);
            scrollView.SetActive(false);
            buttons.SetActive(true);

            BackInputManager.Unregister(closeHandler);
        };

        BackInputManager.Register(closeHandler);
    }

}
