using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Mono.json 데이터 캐싱
    private Dictionary<string, List<string>> monoData;
    public PlayerMove playermove;
    public key_info keyinfo;
    public UnityEngine.UI.Image notification;
    public bool autoMove = false;
    public static GameManager Instance;
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
        while (playermove.starting == false)
        {
            yield return null;
        }
        // 1. 기상
        scenarioState = ScenarioState.WakeUp;
        if (playermove != null) playermove.canInput = false;
        yield return ShowMono("wakeUp", 2f, 1f);
        if (playermove != null) playermove.canInput = true;
        Debug.Log("기상 완료");

        // 2. 움직이기 안내
        if (keyinfo != null) keyinfo.is_starting = true;
        scenarioState = ScenarioState.MoveGuide;
        yield return ShowMono("moveGuide", 2f, 1f);
        Debug.Log("움직이기 안내 완료");

        // 3. 플레이어가 이동할 때까지 대기
        hasMoved = false;
        while (!hasMoved)
        {
            yield return null;
        }
        while (hasObjectActivated == false)
        {
            yield return null;
        }

        // 4. 종이쪼가리 발견
        scenarioState = ScenarioState.FindPaper;
        yield return ShowMono("findPaper", 2f, 1f);


        while (!paperOpened)
        {
            yield return null;
        }

        // 5. 종이쪼가리 확인 + 리액션
        scenarioState = ScenarioState.PaperReaction;
        yield return ShowMono("paperReaction", 2f, 1f);
        autoMove = true;
        //플레이어 침대 자동 리턴
        while (!bedding)
        {
            yield return null;
        }
        // 6. 침대에 누움 + 휴대폰 안내
        scenarioState = ScenarioState.BedRest;
        yield return ShowMono("bedRest", 2f, 1f);

        scenarioState = ScenarioState.PhoneGuide;
        yield return ShowMono("phoneGuide", 2f, 1f);
        playermove.WakeUpExternal();
        // 7. 퀘스트 확인 후(침대에서 일어나기)
        scenarioState = ScenarioState.QuestReaction;
        yield return ShowMono("questReaction", 2f, 1f);
        // 8. 할 일 퀘스트 후, 침대 리턴
        //할일 퀘스트: 카톡 답하기, 노트북 확인, 거울 보기
        while (!(replCount >= 2 && computerChecked && mirrorChecked))
        {
            yield return null;
        }
        // TODO: 에너지 0으로 만들기
        yield return new WaitForSeconds(4f);
        playermove.SleepExternal();
        playermove.canInput = false;
        scenarioState = ScenarioState.AfterQuest;
        yield return ShowMono("afterQuest", 2f, 1f);

        // 9. 침대에 누운 후 문구
        // TODO: 판정문 넣기
        scenarioState = ScenarioState.BedDepressed;
        yield return ShowMono("bedDepressed", 2f, 1f);
        
        // 10. 버스커 연락 (카톡 메시지 연출)
        scenarioState = ScenarioState.BuskerContact;
        notification.enabled = true;
        yield return new WaitForSeconds(2f);
        notification.enabled = false;
        // TODO: 카톡 메시지 UI 연출, 실제 채팅 시스템과 연동 필요
        // yield return ShowBuskerContact();

        scenarioState = ScenarioState.Done;
        // TODO: 챕터2로 전환, 화면 어두워짐 등 연출 필요
    }

    // 외부에서 호출: 플레이어가 이동하면 hasMoved = true
    public void OnPlayerMoved()
    {
        hasMoved = true;
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
            StartCoroutine(ShowMono("findPaper", 2f, 1f));
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

    IEnumerator ShowMono(string key, float showTime, float gapTime)
    {
        if (monoData.ContainsKey(key))
            MonologueManager.Instance.ShowMonologuesSequentially(monoData[key], showTime, gapTime);
        yield return new WaitForSeconds(monoData.ContainsKey(key) ? monoData[key].Count * (showTime + gapTime) : 0f);
    }

    // Mono.json 파싱용 클래스
    [System.Serializable]
    public class MonoDataWrapper
    {
        public List<string> wakeUp;
        public List<string> moveGuide;
        public List<string> findPaper;
        public List<string> paperReaction;
        public List<string> bedGuide;
        public List<string> bedRest;
        public List<string> phoneGuide;
        public List<string> questReaction;
        public List<string> afterQuest;
        public List<string> bedDepressed;

        public Dictionary<string, List<string>> ToDictionary()
        {
            var dict = new Dictionary<string, List<string>>();
            if (wakeUp != null) dict["wakeUp"] = wakeUp;
            if (moveGuide != null) dict["moveGuide"] = moveGuide;
            if (findPaper != null) dict["findPaper"] = findPaper;
            if (paperReaction != null) dict["paperReaction"] = paperReaction;
            if (bedGuide != null) dict["bedGuide"] = bedGuide;
            if (bedRest != null) dict["bedRest"] = bedRest;
            if (phoneGuide != null) dict["phoneGuide"] = phoneGuide;
            if (questReaction != null) dict["questReaction"] = questReaction;
            if (afterQuest != null) dict["afterQuest"] = afterQuest;
            if (bedDepressed != null) dict["bedDepressed"] = bedDepressed;
            return dict;
        }
    }

}
