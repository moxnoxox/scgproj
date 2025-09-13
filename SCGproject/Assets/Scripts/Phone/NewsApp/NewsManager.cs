using UnityEngine;
using UnityEngine.UI;

public class NewsManager : MonoBehaviour
{
    [SerializeField] private GameObject buttons;        // 뉴스 목록 버튼 그룹
    [SerializeField] private GameObject scrollView;     // 뉴스 상세 스크롤 뷰
    [SerializeField] private Button[] newsButtons;      // 뉴스 버튼들
    [SerializeField] private GameObject[] newsPanels;   // NewsPanels 밑에 있는 News1~5
    [SerializeField] private Transform NewsPanels;      // NewsPanels 오브젝트 (원래 부모)

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

        // 모든 뉴스 끄기
        for (int i = 0; i < newsPanels.Length; i++)
            newsPanels[i].SetActive(false);

        // 선택한 뉴스만 켜고 Content 밑으로 옮기기
        RectTransform contentRect = scrollView.transform.Find("Viewport/Content").GetComponent<RectTransform>();
        GameObject selectedNews = newsPanels[index];
        selectedNews.SetActive(true);
        selectedNews.transform.SetParent(contentRect, false);

        // Content 크기를 선택된 뉴스 크기에 맞추기
        RectTransform newsRect = selectedNews.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, newsRect.sizeDelta.y);
        contentRect.anchoredPosition = Vector2.zero;

        // 뒤로가기 핸들러 등록
        System.Action closeHandler = null;
        closeHandler = () =>
        {
            selectedNews.SetActive(false);

            // 다시 원래 부모(NewsPanels) 밑으로 돌려놓기
            selectedNews.transform.SetParent(NewsPanels, false);

            scrollView.SetActive(false);
            buttons.SetActive(true);

            BackInputManager.Unregister(closeHandler);
        };

        BackInputManager.Register(closeHandler);
    }
}
