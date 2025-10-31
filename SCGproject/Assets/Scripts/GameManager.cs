using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Mono.json 데이터 캐싱
    private Dictionary<string, List<string>> monoData;
    public PlayerMove playermove;
    public key_info keyinfo;
    public player_power playerPower;
    public UnityEngine.UI.Image notification;
    public bool autoMove = false;
    public bool phoneOpenEnable = false;
    public bool AfterQuest = false;
    public static GameManager Instance;
    public bool canInput;
    public paper Paper;

    public computer computerScript;  


    //선택지 
    public Transform choicePanel;
    public GameObject choiceButtonPrefab;
    private bool choiceSelected;
    private int selectedIndex;
    

    // 시나리오 상태 관리
    private enum ScenarioState
    {
        WakeUp,
        MoveGuide,
        FindPaper,
        PaperReaction,
        BedGuide,
        BedRest,
        PhoneGuide,
        QuestReaction,
        MirrorScene,
        AfterQuest,
        BedDepressed,
        BuskerContact,
        Done
    }
    private ScenarioState scenarioState;

    private bool hasMoved = false;
    private bool paperOpened = false;
    private bool bedding = false;
    private bool questDone = false;
    private int replCount = 0;
    private bool computerChecked = false;
    private bool mirrorChecked = false;
    public bool hasObjectActivated = false;
    public bool gameStarted = false;
    public bool papaermonologueDone = false;

    void Start()
    {
        // Mono.json 불러오기
        TextAsset monoJson = Resources.Load<TextAsset>("MonologueData/Mono");
        if (monoJson != null)
        {
            monoData = JsonUtility.FromJson<MonoDataWrapper>(monoJson.text).ToDictionary();
        }
        else
        {
            monoData = new Dictionary<string, List<string>>();
        }
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        notification.enabled = false;
        StartCoroutine(ScenarioFlow());
    }

    IEnumerator ScenarioFlow()
    {
        playermove.canInput = false;
        playermove.movable = false;
        while (gameStarted == false)
        {
            yield return null;
        }
        playermove.canInput = true;
        while (playermove.starting == false)
        {
            yield return null;
        }
        // 1. 기상
        scenarioState = ScenarioState.WakeUp;
        if (playermove != null) playermove.canInput = false;
        yield return new WaitForSeconds(2f);
        yield return ShowMono("wakeUp", 2f);
        if (playermove != null) playermove.canInput = true;
        Debug.Log("기상 완료");
        playermove.movable = true;
        // 2. 움직이기 안내
        if (keyinfo != null) keyinfo.is_starting = true;
        scenarioState = ScenarioState.MoveGuide;
        hasMoved = false;
        yield return Showannouncement("moveGuide", 2f);
        Debug.Log("움직이기 안내 완료");

        // 3. 플레이어가 이동할 때까지 대기

        while (!hasMoved)
        {
            yield return null;
        }
        Debug.Log("움직이기 완료");
        while (hasObjectActivated == false)
        {
            yield return null;
        }
        Debug.Log("변수까진 작동함");
        keyinfo.is_starting = false;
        
        // 4. 종이쪼가리 발견
        scenarioState = ScenarioState.FindPaper;
        playermove.movable = false;
        yield return ShowMono("findPaper", 2f);
        yield return Showannouncement("objectGuide", 2f);
        papaermonologueDone = true;
        Paper.canInteractPaper = true; 
  

        while (!paperOpened)
        {
            yield return null;
        }

        // 5. 종이쪼가리 확인 + 리액션
        scenarioState = ScenarioState.PaperReaction;
        yield return StartCoroutine(ShowMono("paperReaction", 2f));
        //플레이어 침대 자동 리턴
        playerPower.DecreasePower(10);
        yield return Showannouncement("bedGuide", 2f);
        playermove.canInput = false;
        yield return new WaitForSeconds(1f);
        // 6. 침대에 누움 + 휴대폰 안내
        scenarioState = ScenarioState.BedRest;
        yield return ShowMono("bedRest", 2f);

        scenarioState = ScenarioState.PhoneGuide;
        yield return Showannouncement("phoneGuide", 2f);
        playermove.WakeUpExternal();
        playermove.movable = true;
        phoneOpenEnable = true;
        playermove.canInput = true;
        playermove.showClickIndicator();
        autoMove = false;
        // 7. 퀘스트 확인 후(침대에서 일어나기)
        scenarioState = ScenarioState.QuestReaction;
        yield return ShowMono("questReaction", 2f);

        //7.5 .. 카톡 + 노트북 확인 완료 시 
        while (!(replCount >= 2 && computerChecked && computerScript.isHomeClosed))
        {
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        yield return ShowMono("afterMonitor", 2f);

        // 8. 할 일 퀘스트 후, 침대 리턴
        //할일 퀘스트: 카톡 답하기, 노트북 확인, 거울 보기
        while (!(replCount >= 2 && computerChecked && mirrorChecked))
        {
            yield return null;
        }
        scenarioState = ScenarioState.MirrorScene;
        playermove.canInput = false;
        playermove.movable = false;
        phoneOpenEnable = false;
        yield return ShowMono("mirrorScene", 2f);
        playerPower.DecreasePower(100);
        yield return new WaitForSeconds(5f);
        yield return ShowMono("afterQuest", 2f);
        autoMove = true;
        yield return new WaitForSeconds(5f);
        playermove.SleepExternal();
        AfterQuest = true;
        autoMove = false;
        phoneOpenEnable = false;
        scenarioState = ScenarioState.AfterQuest;


        // 9. 침대에 누운 후 문구
        scenarioState = ScenarioState.BedDepressed;
        yield return ShowMono("bedDepressed1", 2f);
        playermove.canInput = true;
        playermove.movable = true;
        phoneOpenEnable = true;
        // 10. 버스커 연락 (카톡 메시지 연출)
        scenarioState = ScenarioState.BuskerContact;
        notification.enabled = true;
        yield return new WaitForSeconds(2f);
        notification.enabled = false;
        yield return ShowMono("bedDepressed2", 2f);
        notification.enabled = true;
        yield return new WaitForSeconds(2f);
        notification.enabled = false;
        yield return ShowMono("bedDepressed3", 2f);
        notification.enabled = true;
        yield return new WaitForSeconds(1f);
        notification.enabled = false;
        yield return ShowMono("bedDepressed4", 2f);

        FinalChatTrigger.Instance.StartFinalChat();
        yield return new WaitUntil(() => FinalChatTrigger.Instance.isChatDone);
        yield return ShowMono("afterMessage", 2f);

        ChatRoom currentRoom = ChatAppManager.Instance.chatManager.GetCurrentRoom();
        if (currentRoom != null)
        {
            // 두 번째 자동 대화 JSON 파일명
            string nextJson = "guitar_afterquest2";  // 네가 만든 파일 이름 그대로
            TextAsset jsonFile = Resources.Load<TextAsset>($"ChatData/{nextJson}");

            if (jsonFile != null)
            {
                ChatRoom nextData = JsonUtility.FromJson<ChatRoom>(jsonFile.text);
                Debug.Log($"✅ 자동카톡2 JSON 로드 완료 ({nextData.messages.Count}개 메시지)");

                // 새 메시지만 뽑아서 재생
                yield return StartCoroutine(ChatAppManager.Instance.chatManager.PlayAutoMessages(nextData.messages));
            }
            else
            {
                Debug.LogError($"❌ {nextJson}.json 파일을 찾을 수 없습니다.");
            }
        }

        scenarioState = ScenarioState.Done;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Chapter2");
        // TODO: 챕터2로 전환, 화면 어두워짐 등 연출 필요
    }

    // 외부에서 호출: 플레이어가 이동하면 hasMoved = true
    public void OnPlayerMoved()
    {
        hasMoved = true;
    }
    // 호환성 래퍼: 외부에서 대문자 스타일로 호출할 수 있도록 함
    public void OnPaperRead()
    {
        onPaperOpened();
    }
    public void onPaperOpened()
    {
        paperOpened = true;
    }
    public void onBedding()
    {
        bedding = true;
    }
    public void onQuestDone()
    {
        questDone = true;
    }
    // 외부에서 호출: key_info의 isObject가 처음 true가 될 때
    public void OnObjectActivated()
    {
        if (!hasObjectActivated)
        {
            hasObjectActivated = true;
        }
    }
    public void onReplCount()
    {
        replCount += 1;
    }
    public void onComputerChecked()
    {
        computerChecked = true;
    }
    public void onMirrorChecked()
    {
        mirrorChecked = true;
    }
    public int getreplCount()
    {
        return replCount;
    }
    public bool getComputerChecked()
    {
        return computerChecked;
    }
    public bool getMirrorChecked()
    {
        return mirrorChecked;
    }

    IEnumerator ShowMono(string key, float showTime)
    {
        canInput = false;

        if (monoData.ContainsKey(key))
        {
            List<string> lines = monoData[key];
            foreach (var rawLine in lines)
            {
                // choice 객체 판별: JSON 형태인지 확인
                if (rawLine.TrimStart().StartsWith("{"))
                {
                    ChoiceData choice = JsonUtility.FromJson<ChoiceData>(rawLine);
                    if (choice != null && choice.type == "choice")
                    {
                        yield return ShowChoices(choice.options);
                        int choiceResult = GetChoiceResult();

                        // 선택 결과에 따라 다음 대사 분기
                        if (choiceResult == 0)
                            yield return ShowMono(choice.nextKeys[0], showTime);
                        else if (choiceResult == 1 && choice.nextKeys.Count > 1)
                            yield return ShowMono(choice.nextKeys[1], showTime);

                        yield break;
                    }
                }
                else
                {
                    // 일반 대사 출력
                    MonologueManager.Instance.ShowMonologuesSequentially(new List<string> { rawLine }, showTime);
                    yield return new WaitForSeconds(showTime);
                }
            }
        }

        canInput = true;
    }
    IEnumerator Showannouncement(string key, float showTime)
    {
        canInput = false;
        if (monoData.ContainsKey(key))
            MonologueManager.Instance.ShowAnnouncement(monoData[key], showTime);
        yield return new WaitForSeconds(monoData.ContainsKey(key) ? monoData[key].Count * showTime : 0f);
        canInput = true;
    }

    IEnumerator ShowChoices(List<string> options)
    {
        choiceSelected = false;
        selectedIndex = -1;

        if (!choicePanel.gameObject.activeSelf)
            choicePanel.gameObject.SetActive(true);

        foreach (Transform child in choicePanel)
            Destroy(child.gameObject);

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicePanel);

            // 🔹 Button 컴포넌트 강제 활성화
            var buttonComp = btnObj.GetComponent<Button>();
            if (buttonComp != null)
                buttonComp.enabled = true;

            // 🔹 Image도 혹시 모르니 켜주기
            var img = btnObj.GetComponent<Image>();
            if (img != null)
                img.enabled = true;

            // 🔹 TMP 텍스트 찾기 + 강제 활성화
            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.enabled = true;                 // TMP 컴포넌트 자체가 꺼져 있던 경우
                tmp.gameObject.SetActive(true);     // GameObject 비활성화 대비
                tmp.text = options[i];
            }
            else
            {
                Debug.LogWarning($"TMP 텍스트를 찾지 못했음: {btnObj.name}");
            }

            // 클릭 이벤트 연결
            buttonComp.onClick.AddListener(() => OnChoiceSelected(index));
        }

        yield return new WaitUntil(() => choiceSelected);

        foreach (Transform child in choicePanel)
            Destroy(child.gameObject);

        choicePanel.gameObject.SetActive(false);
    }



    void OnChoiceSelected(int index)
    {
        selectedIndex = index;
        choiceSelected = true;
    }

    public int GetChoiceResult()
    {
        return selectedIndex;
    }
    public string GetCurrentScenarioState()
    {
        return scenarioState.ToString();
    }

    private IEnumerator StartAutoMoveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        autoMove = true;
    }


    // Mono.json 파싱용 클래스
    [System.Serializable]
    public class MonoDataWrapper
    {
        public List<string> wakeUp;
        public List<string> moveGuide;
        public List<string> findPaper;
        public List<string> objectGuide;
        public List<string> paperReaction;
        public List<string> bedGuide;
        public List<string> bedRest;
        public List<string> phoneGuide;
        public List<string> questReaction;
        public List<string> afterMonitor;
        public List<string> afterQuest;
        public List<string> bedDepressed1;
        public List<string> bedDepressed2;
        public List<string> bedDepressed3;
        public List<string> bedDepressed4;
        public List<string> mirrorScene;
        public List<string> afterMessage;
        public List<string> afterMessage2;

        public Dictionary<string, List<string>> ToDictionary()
        {
            var dict = new Dictionary<string, List<string>>();
            if (wakeUp != null) dict["wakeUp"] = wakeUp;
            if (moveGuide != null) dict["moveGuide"] = moveGuide;
            if (findPaper != null) dict["findPaper"] = findPaper;
            if (objectGuide != null) dict["objectGuide"] = objectGuide;
            if (paperReaction != null) dict["paperReaction"] = paperReaction;
            if (bedGuide != null) dict["bedGuide"] = bedGuide;
            if (bedRest != null) dict["bedRest"] = bedRest;
            if (phoneGuide != null) dict["phoneGuide"] = phoneGuide;
            if (questReaction != null) dict["questReaction"] = questReaction;
            if (afterMonitor != null) dict["afterMonitor"] = afterMonitor;
            if (afterQuest != null) dict["afterQuest"] = afterQuest;
            if (bedDepressed1 != null) dict["bedDepressed1"] = bedDepressed1;
            if (bedDepressed2 != null) dict["bedDepressed2"] = bedDepressed2;
            if (bedDepressed3 != null) dict["bedDepressed3"] = bedDepressed3;
            if (bedDepressed4 != null) dict["bedDepressed4"] = bedDepressed4;
            if (mirrorScene != null) dict["mirrorScene"] = mirrorScene;
            if (afterMessage != null) dict["afterMessage"] = afterMessage;
            if (afterMessage2 != null) dict["afterMessage2"] = afterMessage2;
            return dict;
        }
    }

    [System.Serializable]
    public class ChoiceData
    {
        public string type;
        public List<string> options;
        public List<string> nextKeys;
    }
}
