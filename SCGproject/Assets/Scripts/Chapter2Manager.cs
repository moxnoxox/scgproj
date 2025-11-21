using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 추가
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq; // Linq 네임스페이스 추가

public class Chapter2Manager : MonoBehaviour
{
    public static Chapter2Manager Instance;

    // --- 기본 참조 변수들 ---
    public PlayerMove playerMove;
    public player_power playerPower;
    public QuestManagerCh2 questManager; // 챕터2 퀘스트 매니저

    // --- UI 참조 변수들 ---
    public Transform choicePanel; // 선택지 버튼들이 생성될 부모 Panel
    public GameObject choiceButtonPrefab; // 선택지 버튼 프리팹
    public UnityEngine.UI.Image endingImage;

    // --- 시나리오 진행 관련 변수들 ---
    public bool autoMove; // 플레이어 자동 이동 활성화 여부
    public bool ch2_movable;
    public bool ch2_canSleep;
    private Dictionary<string, List<string>> monoData; // Mono2.json 데이터 저장
    private ScenarioState scenarioState; // 현재 시나리오 단계

    // --- 조건 체크용 플래그 변수들 ---
    private bool playerNearTutorialTrashBag = false; // [수정] 튜토리얼 대상 2단 쓰봉 근처 여부
    private bool playerNearTrashCan = false; // 쓰레기통 근처 여부
    private bool playerNearUSB = false; // USB 근처 여부 (튜토리얼 외 일반 USB 감지용)
    private bool usbQuestTriggered = false; // USB 줍기 퀘스트 완료 여부
    private bool laptopOpened = false; // 노트북 열람 여부
    private bool fileSortGameDone = false; // 파일 정렬 미니게임 완료 여부
    private bool guitarBodyFound = false; // 기타 본체 발견 여부
    public bool guitarCaseFound = false; // 기타 케이스 발견 여부
    private bool peakFound = false; // 피크 발견 여부
    private bool stringFound = false;
    private bool paperPuzzleDone = false; // 종이 퍼즐 미니게임 완료 여부
    private bool guitarPartsAllFound = false; // 모든 기타 부품 발견 여부 (챕터 종료 조건)

    // --- 선택지 관련 변수들 ---
    private bool choiceSelected; // 선택지가 선택되었는지 여부
    private int selectedIndex; // 선택된 선택지의 인덱스
    public bool canHold = false;

    // --- 연출용 오브젝트 참조 ---
    public GameObject trashCanObject; // Inspector에서 등장할 쓰레기통 오브젝트 연결
    public Transform tutorialTargetTrashBag; // Inspector에서 튜토리얼 대상 2단 쓰봉 오브젝트 연결

    // --- 외부 매니저 참조 ---
    public PhonePanelController phoneController;
    public ChatAppManager chatAppManager;
    // public CameraController cameraController; // 필요 시 주석 해제 및 연결
    // public IllustrationManager illustrationManager; // 필요 시 주석 해제 및 연결

    // --- 내부 상태 변수 ---
    private bool trashTutorialSkipped = false; // 쓰레기 튜토리얼 건너뛰기 여부

    // --- 시나리오 단계 Enum ---
    private enum ScenarioState
    {
        StartContact,       // 시작 독백
        ShowPhoto,          // 카톡 확인
        DecideToSearch,     // 기타 찾기 결심, 퀘스트 생성
        TrashBagApproach,   // 쓰레기 튜토리얼 시작
        TrashTutorialChoices, // 튜토리얼 선택지
        TrashTutorialExecution, // 튜토리얼 진행 (쓰봉 들고 버리기)
        FreeMove,           // 자유 이동 시작
        USBInteraction,     // (자유 이동 중) USB 상호작용
        LaptopOpened,       // (자유 이동 중) 노트북 열기
        FileSortGameStart,  // (자유 이동 중) 파일 정렬 시작
        FileSortGameComplete,// (자유 이동 중) 파일 정렬 완료
        GuitarBodyFound,    // (자유 이동 중) 기타 본체 발견
        GuitarCaseFound,    // (자유 이동 중) 기타 케이스 발견
        PaperPuzzleStart,   // (자유 이동 중) 종이 퍼즐 시작
        PaperPuzzleComplete,// (자유 이동 중) 종이 퍼즐 완료
        AllGuitarPartsFound,// 모든 부품 찾음 (자유 이동 종료 조건)
        EndingTransition    // 엔딩 시작
    }

    // --- 초기화 ---
    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        ch2_movable = false;
    }

    void Start()
    {
        // Mono2.json 데이터 로드
        LoadMonologueData();

        // 연출용 오브젝트 초기 상태 설정
        if (trashCanObject != null)
            trashCanObject.SetActive(false); // 쓰레기통 숨기기

        // 필수 매니저 참조 확인 및 가져오기
        FindEssentialManagers();
        if (playerMove == null || phoneController == null || chatAppManager == null )
        {
             Debug.LogError("필수 매니저 중 하나 이상을 찾거나 연결할 수 없습니다. 챕터 진행을 중단합니다.");
             return; // 필수 매니저 없으면 시작 중단
        }

        if(endingImage != null)
            endingImage.enabled = false; // 엔딩 이미지 숨기기
        // 메인 시나리오 코루틴 시작
        StartCoroutine(ScenarioFlow());
    }

    // --- Monologue 데이터 로드 함수 ---
    void LoadMonologueData()
    {
        TextAsset monoJson = Resources.Load<TextAsset>("MonologueData/Mono2");
        if (monoJson != null)
        {
            try
            {
                monoData = JsonUtility.FromJson<MonoDataWrapper>(monoJson.text).ToDictionary();
                Debug.Log($"Mono2.json 로드 완료. 총 {monoData.Count}개의 키를 읽음");
            }
            catch (Exception e)
            {
                Debug.LogError($"Mono2.json 파싱 오류: {e.Message}");
                monoData = new Dictionary<string, List<string>>();
            }
        }
        else
        {
            monoData = new Dictionary<string, List<string>>();
            Debug.LogWarning("Mono2.json을 찾을 수 없습니다. 경로 Resources/MonologueData/ 를 확인하세요.");
        }
    }

    // --- 필수 매니저 찾기 함수 ---
    void FindEssentialManagers() {
         if (playerMove == null) playerMove = FindFirstObjectByType<PlayerMove>();
         if (playerPower == null && playerMove != null) playerPower = playerMove.GetComponent<player_power>(); // PlayerMove에서 가져오기
         if (questManager == null) questManager = FindFirstObjectByType<QuestManagerCh2>();
         if (phoneController == null) phoneController = FindFirstObjectByType<PhonePanelController>();
         if (chatAppManager == null) chatAppManager = FindFirstObjectByType<ChatAppManager>();

         // 필수 매니저 누락 시 에러 로그 (playerPower, questManager는 없을 수도 있음)
         if (playerMove == null) Debug.LogError("PlayerMove 참조를 찾을 수 없습니다!");
         if (phoneController == null) Debug.LogError("PhonePanelController 참조를 찾을 수 없습니다!");
         if (chatAppManager == null) Debug.LogError("ChatAppManager 참조를 찾을 수 없습니다!");
    }


    // --- 메인 시나리오 흐름 코루틴 ---
    IEnumerator ScenarioFlow()
    {
        yield return new WaitForSeconds(2f);
        // 1. 선형적인 시작 부분 (인트로)
        scenarioState = ScenarioState.StartContact;
        ch2_canSleep = false;
        yield return ShowMono("ch2start", 2f); // "일단 찾겠다고는...", "막막하네"

        // 2. 카톡 자동 진행
        scenarioState = ScenarioState.ShowPhoto;
        Debug.Log("1. 카톡 자동 열림 시작");
        // 버스커 카톡 넣기 ???????
        playerPower?.IncreasePower(10); // 에너지 증가는 카톡 후

        // 3. 줌아웃 및 독백
        yield return ShowMono("postChatMono", 2f); // "……무작정 바닥에서...", "쓰레기봉지 때문에..."
        ch2_canSleep = true;
        Debug.Log("카메라 줌아웃 - 집 전체 클로즈업 (연출 필요)");
        // 예: cameraController?.ZoomOutToShowHouse();
        yield return new WaitForSeconds(2f); // 줌아웃 연출 대기 시간

        // 4. 기타 찾기 결심 및 퀘스트 생성
        scenarioState = ScenarioState.DecideToSearch;
        yield return ShowMono("cleanDecision", 2f); // "흠…기타도 찾을 겸...", "집 청소나 해 볼까?"
        Debug.Log("3. 퀘스트 추가: 기타 찾기");
        questManager?.UpdateGuitarQuest();
        yield return ShowAnnouncementByKey("todoalarm1", 2f); // [수정] 함수 이름 변경


        // --- [수정됨] 상세한 쓰레기 튜토리얼 시퀀스 ---
        scenarioState = ScenarioState.TrashBagApproach;
        yield return ShowMono("trashTutorial_Start", 2f); // "사방에...", "밀어볼까?..."
        ch2_movable = true;
        // 튜토리얼 대상 2단 쓰봉으로 이동 안내 (포인터 연출 가정)
        // tutorialManager?.ShowPointer(tutorialTargetTrashBag);
        Debug.Log("튜토리얼: 지정된 2단 쓰레기봉지로 이동하세요.");

        // 플레이어가 '튜토리얼 대상' 2단 쓰레기봉지 근처로 갈 때까지 대기
        yield return new WaitUntil(() => playerNearTutorialTrashBag);
        // tutorialManager?.HidePointer();
        Debug.Log("튜토리얼: 2단 쓰레기봉지 도착.");

        // 밀기 실패 및 쓰레기통 찾기 독백
        yield return ShowMono("trashTutorial_Fail", 2f);
        yield return ShowMono("trashTutorial_FindCan", 2f);
        yield return new WaitForSeconds(2f);

        // 쓰레기통 등장 연출
        Debug.Log("쓰레기통 등장 (뚜둔)");
        if (trashCanObject != null) trashCanObject.SetActive(true);
        yield return ShowMono("trashTutorial_CanFound", 2f);

        // 튜토리얼 안내: 스페이스바 누르기
        yield return ShowAnnouncement(new List<string> { "2단 쓰레기봉지에 다가가 스페이스바를 눌러 보세요." }, 3f);
        scenarioState = ScenarioState.TrashTutorialChoices;

        // 선택지 표시: '치우자' / '돌아가자'
        // [수정] ShowChoices가 코루틴이므로 yield return 사용
        yield return ShowChoices(new List<string> { "> 쓰레기봉지를 치우자", "> 그냥 돌아서 가자" });
        int choiceResult = GetChoiceResult();

        if (choiceResult == 0) // '쓰레기봉지를 치우자' 선택
        {
            trashTutorialSkipped = false;
            scenarioState = ScenarioState.TrashTutorialExecution;
            Debug.Log("튜토리얼: '쓰레기봉지를 치우자' 선택됨.");
            // 안내: '**‘쓰레기봉지를 치우자’**를 눌러 보세요." (선택했으므로 다음 단계 안내)
            canHold = true;
            Debug.Log("플레이어가 물체를 들 수 있는 상태(canHold = true)로 변경됨.");
            yield return ShowAnnouncement(new List<string> { "<b>‘쓰레기봉지를 치우자’</b>를 선택했습니다." , "스페이스바를 눌러 쓰레기봉지를 드세요."}, 4f);


            // 플레이어가 스페이스바를 눌러 쓰봉 들기를 기다림
            yield return new WaitUntil(() => playerMove != null && playerMove.isHolding);
            Debug.Log("튜토리얼: 쓰레기봉지를 들었습니다.");

            // 안내: 쓰레기통 이동
            yield return ShowAnnouncement(new List<string> { "쓰레기통 근처로 이동해 보세요." }, 3f);

            // 플레이어가 쓰레기통 근처로 가기를 기다림
            yield return new WaitUntil(() => playerNearTrashCan);
            Debug.Log("튜토리얼: 쓰레기통 도착.");

            // 안내: 스페이스바로 버리기
            yield return ShowAnnouncement(new List<string> { "스페이스바를 눌러 쓰레기봉지를 버리세요." }, 4f);

            // 플레이어가 스페이스바를 눌러 쓰봉 버리기를 기다림
            yield return new WaitUntil(() => playerMove != null && !playerMove.isHolding);
            Debug.Log("튜토리얼: 쓰레기봉지를 버렸습니다.");

            // 튜토리얼 완료 독백
            yield return ShowAnnouncement(monoData["trashTutorial_End"], 2f);
        }
        else // '그냥 돌아서 가자' 선택
        {
            trashTutorialSkipped = true;
            scenarioState = ScenarioState.TrashTutorialChoices;

            Debug.Log("튜토리얼: '그냥 돌아서 가자' 선택됨.");

            // 💬 회피 반응 대사
            yield return ShowMono("trashTutorial_Skip", 2f);
            // ex) "치우기 싫다... 그냥 두자."

            // ex) "…근데, 이걸 안 치우면 지나갈 수가 없잖아."

            // ex) "하… 알겠어. 그냥 치우자."

            canHold = true;
            scenarioState = ScenarioState.TrashTutorialExecution;
            Debug.Log("플레이어가 결국 쓰레기봉지를 치우기로 결심함 (canHold = true)");

            yield return ShowAnnouncement(new List<string> { "스페이스바를 눌러 쓰레기봉지를 드세요." }, 3f);
            
            // 플레이어가 스페이스바를 눌러 쓰봉 버리기를 기다림
            yield return new WaitUntil(() => playerMove != null && !playerMove.isHolding);
            Debug.Log("튜토리얼: 쓰레기봉지를 버렸습니다.");

            // 튜토리얼 완료 독백
            yield return ShowAnnouncement(monoData["trashTutorial_End"], 2f);
        }

        // --- 쓰레기 튜토리얼 시퀀스 끝 ---


        // 5. 자유 이동 시작
        scenarioState = ScenarioState.FreeMove;
        yield return ShowMono("trashTutorial_ToFreeMove", 2f); // "아까부터...", "치우면서..."
        Debug.Log("--- 챕터 2 '자유 이동' 구간 시작 ---");

        // 모든 기타 부품을 찾을 때까지 대기 (각 부품 스크립트가 OnGuitarPartsAllFound 호출)
        yield return new WaitUntil(() => guitarPartsAllFound);


        // 6. 선형적인 종료 부분 (엔딩)
        Debug.Log("--- '자유 이동' 구간 종료 ---");
        scenarioState = ScenarioState.EndingTransition;

        // 엔딩 독백 및 연출
        yield return ShowMono("ending_AllFound", 2f); // "얼추 다...", "줄부터..."
        yield return new WaitForSeconds(2f); // 줄 바꾸는 시간
        yield return ShowMono("ending_Tune", 2f); // "오랜만에...", "...쳐볼까?"

        Debug.Log("기타 치는 일러스트 표시 (연출 필요)");
        yield return EndingImageFadeInCoroutine();
        // 예: illustrationManager?.Show("GuitarPlaying");
        yield return new WaitForSeconds(1f); // 일러스트 표시 시간

        yield return ShowMono("ending_Play1", 2f);
        yield return ShowMono("ending_Play2", 2f);
        yield return ShowMono("ending_Play3", 2f);
        yield return new WaitForSeconds(2f); // 독백 사이 멈춤
        yield return ShowMono("ending_Realization", 2f);
        yield return ShowMono("ending_Final", 2f);
        yield return new WaitForSeconds(3f); // 여운

        // 챕터 3 전환
        BackInputManager.ClearAll();
        Debug.Log("일러스트 종료, 챕터 3 로드 (연출 필요)");
        SceneController.Loadscene("Chapter3");
        // 예: illustrationManager?.Hide();
        // SceneManager.LoadScene("Chapter3"); // 실제 씬 이름으로 변경
    }

     // [추가] 초기 메시지의 총 지연 시간 계산 (ChatManager 확인 실패 시 대체)
    private float CalculateTotalDelay(List<Message> messages)
    {
        float total = 0f;
        if (messages != null)
        {
            // delayAfter가 있는 메시지만 계산
            foreach (var msg in messages.Where(m => m.delayAfter > 0))
            {
                total += msg.delayAfter;
            }
        }
        // 기본 대기 시간도 고려 (메시지당 1초 가정)
        total += messages?.Count ?? 0 * 1.0f;
        return total;
    }


    // --- 독백, 안내, 선택지 표시 함수들 ---
    IEnumerator ShowMono(string key, float showTime)
    {
        if (string.IsNullOrEmpty(key) || !monoData.ContainsKey(key))
        {
            Debug.LogWarning($"모놀로그 키 '{key}'를 찾을 수 없거나 유효하지 않습니다.");
            yield break;
        }

        List<string> lines = monoData[key];
        foreach (var rawLine in lines)
        {
            // 선택지 처리 (JSON 형태 확인)
            if (rawLine.TrimStart().StartsWith("{"))
            {
                ChoiceData choice = null;
                try { choice = JsonUtility.FromJson<ChoiceData>(rawLine); } catch {}

                if (choice != null && choice.type == "choice")
                {
                    yield return ShowChoices(choice.options); // 선택지 표시 코루틴 호출
                    int choiceResult = GetChoiceResult();
                    // 선택 결과에 따른 분기 처리 (필요 시 구현)
                    Debug.Log($"선택 결과: {choiceResult}");
                    yield break; // 선택지 처리 후에는 해당 ShowMono 종료
                }
                else {
                     Debug.LogWarning($"잘못된 JSON 형식 또는 타입이 'choice'가 아님: {rawLine}");
                     // 일반 텍스트로 처리
                     MonologueManager.Instance?.ShowMonologuesSequentially(new List<string> { rawLine }, showTime);
                     yield return new WaitForSeconds(showTime);
                }
            }
            else // 일반 텍스트
            {
                MonologueManager.Instance?.ShowMonologuesSequentially(new List<string> { rawLine }, showTime);
                yield return new WaitForSeconds(showTime); // 각 줄 표시 시간만큼 대기
            }
        }
    }

    // [수정] 키(key)를 받아 공지 표시 (Showannouncement -> ShowAnnouncementByKey)
    IEnumerator ShowAnnouncementByKey(string key, float showTime)
    {
         if (string.IsNullOrEmpty(key) || !monoData.ContainsKey(key))
         {
             Debug.LogWarning($"공지 키 '{key}'를 찾을 수 없거나 유효하지 않습니다.");
             yield break;
         }
         yield return ShowAnnouncement(monoData[key], showTime); // List<string> 버전 호출
    }

    // [추가] List<string>을 직접 받아 공지 표시
    IEnumerator ShowAnnouncement(List<string> messages, float durationPerLine)
    {
        if (messages == null || messages.Count == 0) yield break;
        MonologueManager.Instance?.ShowAnnouncement(messages, durationPerLine);
        // 모든 메시지가 표시될 때까지 대기
        yield return new WaitForSeconds(messages.Count * durationPerLine);
    }


    IEnumerator ShowChoices(List<string> options)
    {
        if (options == null || options.Count == 0 || choicePanel == null || choiceButtonPrefab == null)
        {
            Debug.LogError("선택지 표시에 필요한 요소가 부족합니다.");
            choiceSelected = true; // 진행 막힘 방지
            selectedIndex = -1;
            yield break;
        }

        choiceSelected = false;
        selectedIndex = -1;
        choicePanel.gameObject.SetActive(true);

        // 기존 버튼 삭제
        foreach (Transform child in choicePanel) Destroy(child.gameObject);

        // 새 버튼 생성
        for (int i = 0; i < options.Count; i++)
        {
            int index = i; // 클로저 캡처 방지
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicePanel);

            // 버튼 텍스트 설정
            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>(true); // 비활성화된 자식 포함 검색
            if (tmp != null)
            {
                 tmp.enabled = true; // TMP 컴포넌트 활성화
                 tmp.gameObject.SetActive(true); // GameObject 활성화
                 tmp.text = options[i];
            } else { Debug.LogWarning("선택지 버튼 프리팹에서 TextMeshProUGUI를 찾을 수 없습니다."); }


            // 버튼 컴포넌트 활성화 및 이벤트 연결
            var buttonComp = btnObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                 buttonComp.enabled = true; // 버튼 활성화
                 buttonComp.onClick.AddListener(() => OnChoiceSelected(index));
            } else { Debug.LogWarning("선택지 버튼 프리팹에서 Button 컴포넌트를 찾을 수 없습니다."); }

             // 이미지 컴포넌트 활성화 (필요 시)
             var img = btnObj.GetComponent<Image>();
             if (img != null) img.enabled = true;
        }

        // 선택 대기
        yield return new WaitUntil(() => choiceSelected);

        // 선택 후 패널 숨기기 및 버튼 삭제
        foreach (Transform child in choicePanel) Destroy(child.gameObject);
        choicePanel.gameObject.SetActive(false);
    }

    void OnChoiceSelected(int index) { selectedIndex = index; choiceSelected = true; }
    public int GetChoiceResult() { return selectedIndex; }


    // --- 이벤트 핸들러 함수들 (On... / Play...) ---

    // 튜토리얼 대상 쓰봉 근처 상태 변경 시 호출 (TutorialTrashBagTrigger.cs)
    public void OnPlayerNearTutorialTrashBagChanged(bool isNear) {
         playerNearTutorialTrashBag = isNear;
         Debug.Log($"플레이어 튜토리얼 쓰봉 근처 상태 변경: {isNear}");
    }

    // 쓰레기통 근처 상태 변경 시 호출 (TrashCan.cs)
    public void OnPlayerNearTrashCanChanged(bool isNear)
    {
        playerNearTrashCan = isNear;
        Debug.Log($"플레이어 쓰레기통 근처 상태 변경: {isNear}");
    }

    // 일반 USB 근처 상태 변경 시 호출 (USB.cs)
    public void OnPlayerNearUSB() => playerNearUSB = true; // 이 함수는 현재 로직에서 직접 사용되지 않을 수 있음

    // USB 상호작용 시 호출 (USB.cs)
    public void OnUSBInteracted(int usbIndex) {
        if (!usbQuestTriggered) // 최초 상호작용 시
        {
            usbQuestTriggered = true; // 플래그 설정
            Debug.Log($"USB {usbIndex} 획득. 퀘스트 완료 + 노트북 퀘스트 추가");
            playerPower?.IncreasePower(10);
            questManager?.UpdateComputerQuest(); // 퀘스트 업데이트

            // 노트북을 이미 열어봤다면 바로 파일 정렬 게임 시작
        }
    }

    // 노트북 상호작용 시 호출 (computer_ch2.cs)
    public void OnLaptopOpened() {
        if (laptopOpened) return; // 중복 실행 방지
        laptopOpened = true;

        if (usbQuestTriggered) // USB를 이미 찾았다면
        {
            StartFileSortGame(); // 파일 정렬 게임 시작
        }
        else // USB가 없다면
        {
             Debug.Log("노트북을 열었지만 USB가 필요해 보인다.");
             StartCoroutine(ShowMono("laptop_need_usb", 2f)); // 안내 독백
        }
    }

    // 파일 정렬 게임 시작 함수
    private void StartFileSortGame()
    {
        if (fileSortGameDone) return; // 이미 완료했다면 시작하지 않음
        ch2_movable = false;
        scenarioState = ScenarioState.FileSortGameStart;
        Debug.Log("노트북 열림 → 미니게임(파일정리) 시작");
        // 예: FileSortGameManager.Instance.ShowGameUI(); // 실제 미니게임 시작 호출
        StartCoroutine(WaitForFileSortLogic()); // 완료 대기 코루틴 시작
    }

    // 파일 정렬 게임 완료 대기 코루틴
    IEnumerator WaitForFileSortLogic()
    {
        Debug.Log("파일 정렬 게임 진행 중... (완료 대기 - 실제 구현 필요)");
        // 예: yield return new WaitUntil(() => FileSortGameManager.Instance.IsGameFinished());

        // --- 임시 코드: 즉시 완료 처리 ---
        FileSortGameManager.Instance.ShowGameUI();
        while(!fileSortGameDone)
        {
            yield return null;
        }
        // --- 임시 코드 끝 ---
    }

    // 파일 정렬 게임 완료 시 호출될 함수 (미니게임 매니저가 호출)
    public void OnFileSortGameDone() {
         if(fileSortGameDone) return; // 중복 방지
         fileSortGameDone = true;
         scenarioState = ScenarioState.FileSortGameComplete;
         Debug.Log("파일 정렬 미니게임 완료! 에너지 +10");
         ch2_movable = true;
         playerPower?.IncreasePower(10);
         questManager?.CompleteComputerQuest(); // 퀘스트 완료 처리
         // 파일 정렬 완료 후 독백 등 추가 가능
         // 예: StartCoroutine(ShowMono("laptop_open", 2f));
    }

    // 기타 본체 발견 시 호출 (별도 상호작용 스크립트 필요)
    public void OnGuitarBodyFound()
    {
        if (guitarBodyFound) return;
        guitarBodyFound = true;
        scenarioState = ScenarioState.GuitarBodyFound;
        StartCoroutine(GuitarBodyMonoCoroutine());
        Debug.Log("기타 본체 발견! 퀘스트 추가: 줄, 피크, 케이스 찾기");
        questManager?.CompleteGuitarQuest(); // '기타 찾기' 퀘스트 완료
        questManager?.GuitarPartsFind();
        // 모든 부품 찾기 완료 조건 체크
        CheckAllPartsFound();
    }
    IEnumerator GuitarBodyMonoCoroutine()
    {
        yield return ShowMono("guitar_found", 2f);
        yield return ShowChoices(new List<string> { "> 들어올린다.", ">힘껏 들어올린다." });
        int choiceResult = GetChoiceResult();
        yield return ShowMono("guitar_found2", 2f);
    }

    // 기타 케이스 발견 시 호출 (guitar_case.cs)
    public void OnGuitarCaseFound() {
         if (guitarCaseFound) return;
         scenarioState = ScenarioState.GuitarCaseFound;
         Debug.Log("기타 케이스 찾음 → 종이 퍼즐 미니게임 시작");
         StartPaperPuzzleGame(); // 퍼즐 게임 시작 함수 호출
         // 모든 부품 찾기 완료 조건 체크
         CheckAllPartsFound();
    }

    public void OnPeakFound()
    {
        if (peakFound) return;
        peakFound = true;
        Debug.Log("피크 발견!");
        // 모든 부품 찾기 완료 조건 체크
        CheckAllPartsFound();
    }
    public void OnStringFound()
    {
        if (stringFound) return;
        stringFound = true;
        Debug.Log("기타 줄 발견!");
        // 모든 부품 찾기 완료 조건 체크
        CheckAllPartsFound();
    }

    // [추가] 다른 기타 부품 발견 시 호출될 함수 (예시)
    // public void OnGuitarStringsFound() { /* ... 플래그 설정, CheckAllPartsFound() 호출 ... */ }
    // public void OnGuitarTunerFound() { /* ... 플래그 설정, CheckAllPartsFound() 호출 ... */ }

    // 종이 퍼즐 게임 시작 함수
    private void StartPaperPuzzleGame() {
         if (paperPuzzleDone) return; // 이미 완료했다면 시작 안 함
         ch2_movable = false;
         scenarioState = ScenarioState.PaperPuzzleStart;
         Debug.Log("종이 퍼즐 미니게임 시작");
         // 예: PaperpuzzleController.Instance.StartPuzzle(); // 실제 퍼즐 시작 호출
         StartCoroutine(WaitForPaperPuzzleLogic()); // 완료 대기 코루틴 시작
    }


    // 종이 퍼즐 게임 완료 대기 코루틴
    IEnumerator WaitForPaperPuzzleLogic()
    {
        Debug.Log("종이 퍼즐 게임 진행 중... (완료 대기 - 실제 구현 필요)");
        // 예: yield return new WaitUntil(() => PaperpuzzleController.Instance.isCompleted);
        yield return ShowMono("guitar_case_found", 2f);
        PaperpuzzleController.Instance.StartPuzzle();
        while(!paperPuzzleDone)
        {
            yield return null;
        }
    }

    // 종이 퍼즐 게임 완료 시 호출될 함수 (퍼즐 컨트롤러가 호출)
    public IEnumerator OnPaperPuzzleDone() {
         if (paperPuzzleDone) yield break; // 중복 방지
         paperPuzzleDone = true;
         scenarioState = ScenarioState.PaperPuzzleComplete;
         Debug.Log("종이 퍼즐 미니게임 완료! 갤러리 해금 + 에너지 +10");
         Vector3 targetPosition = MonologueManager.Instance.monologuePanel.transform.position;
         targetPosition.y -= 40;
         MonologueManager.Instance.monologuePanel.transform.position = targetPosition;
         yield return ShowMono("paper_done", 2f);
         targetPosition.y += 40;
         MonologueManager.Instance.monologuePanel.transform.position = targetPosition;
         PaperpuzzleController.Instance.ExittooltipOn();
         playerPower?.IncreasePower(10);
         // 갤러리 해금 로직 (GalleryManager 연동 필요)
         // 예: FindFirstObjectByType<GalleryManager>()?.UnlockPhoto(photoIndexToUnlock);
    }

    // 모든 기타 부품 찾기 완료 조건 체크 함수
    private void CheckAllPartsFound() {
         // 필요한 모든 부품의 발견 플래그 확인
         // 예: if (guitarBodyFound && guitarCaseFound && guitarStringsFound && guitarTunerFound)
         if (guitarBodyFound && guitarCaseFound && peakFound && stringFound/* && 다른 부품 플래그들... */)
         {
            OnGuitarPartsAllFound(); // 모든 부품 찾음 처리 함수 호출
         }
    }

    public void OnMusicInteract1()
    {
        StartCoroutine(Music1());
    }
    IEnumerator Music1() 
    {
        yield return ShowMono("music1", 2f);
    }
    public void OnMusicInteract2()
    {
        StartCoroutine(Music2());
    }
    IEnumerator Music2() 
    {
        yield return ShowMono("music2", 2f);
    }
    public void OnMusicInteract3()
    {
        StartCoroutine(Music3());
    }
    IEnumerator Music3() 
    {
        yield return ShowMono("music3", 2f);
    }


    // 모든 기타 부품 찾기 완료 시 호출될 함수 (CheckAllPartsFound에서 호출)
    public void OnGuitarPartsAllFound() {
        if (guitarPartsAllFound) return; // 중복 방지
        guitarPartsAllFound = true; // 최종 플래그 설정 (ScenarioFlow의 대기 종료 조건)
        Debug.Log("모든 기타 파츠 발견! ScenarioFlow가 종료 시퀀스를 시작합니다.");
        // questManager?.CompleteGuitarPartsQuest(); // 부품 찾기 퀘스트 완료 처리
    }

    // USB 관련 독백 함수들
    public void PlayUSBFirstDialogue(int usbIndex)
    {
        string key = $"usb{usbIndex}_first";
        Debug.Log($"USB {usbIndex} 첫 발견 대사 재생 ({key})");
        if (monoData.ContainsKey(key))
            StartCoroutine(ShowMono(key, 2f));
        else
            Debug.LogWarning($"USB 대사 키 '{key}'를 Mono2.json에서 찾을 수 없습니다.");
    }

    public void PlayUSBRereadHint()
    {
        MonologueManager.Instance?.ShowMonologuesSequentially(
            new List<string> { "이미 확인한 USB다. 노트북에서 확인해보면 될 것 같다." },
            2f
        );
    }

    IEnumerator EndingImageFadeInCoroutine()
    {
        if (endingImage == null) yield break;

        float duration = 2f; // 페이드인 지속 시간
        float elapsed = 0f;
        Color color = endingImage.color;
        color.a = 0f;
        endingImage.color = color;
        endingImage.enabled = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / duration);
            endingImage.color = color;
            yield return null;
        }

        color.a = 1f;
        endingImage.color = color;
    }
    IEnumerator EndingImageFadeOutCoroutine()
    {
        if (endingImage == null) yield break;

        float duration = 2f; // 페이드인 지속 시간
        float elapsed = 0f;
        Color color = endingImage.color;
        color.a = 1f;
        endingImage.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / duration);
            endingImage.color = color;
            yield return null;
        }

        color.a = 0f;
        endingImage.color = color;
        endingImage.enabled = false;
    }

    // --- JSON 데이터 래퍼 클래스 및 Enum 정의 ---

    // MonoDataWrapper 클래스 (키 정의)
    [System.Serializable]
    public class MonoDataWrapper
    {
        // 1. 시작 시퀀스
        public List<string> ch2start;
        public List<string> postChatMono;
        public List<string> cleanDecision;
        public List<string> todoalarm1;

        // 2. 튜토리얼 시퀀스
        public List<string> trashTutorial_Start;
        public List<string> trashTutorial_Fail;
        public List<string> trashTutorial_FindCan;
        public List<string> trashTutorial_CanFound;
        public List<string> trashTutorial_Skip;
        public List<string> trashTutorial_End;
        public List<string> trashTutorial_ToFreeMove;
        public List<string> guitar_found;
        public List<string> guitar_found2;

        // 3. 자유 이동 중 상호작용
        public List<string> usb1_first;
        public List<string> usb2_first;
        public List<string> usb3_first;
        public List<string> laptop_open; // 파일 정렬 완료 후 독백 (선택적)
        public List<string> laptop_need_usb; // USB 없을 때 독백
        public List<string> guitar_case_found;
        public List<string> guitar_case_found2;
        public List<string> guitar_peak_found;
        public List<string> guitar_peak_found2;
        public List<string> paper_done;
        public List<string> music1;
        public List<string> music2;
        public List<string> music3;
        // 4. 엔딩 시퀀스
        public List<string> ending_AllFound;
        public List<string> ending_Tune;
        public List<string> ending_Play1;
        public List<string> ending_Play2;
        public List<string> ending_Play3;
        public List<string> ending_Realization;
        public List<string> ending_Final;

        // 딕셔너리 변환 함수
        public Dictionary<string, List<string>> ToDictionary()
        {
            var dict = new Dictionary<string, List<string>>();
            // 각 List가 null이 아닐 경우 Dictionary에 추가
            if (ch2start != null) dict.Add("ch2start", ch2start);
            if (postChatMono != null) dict.Add("postChatMono", postChatMono);
            if (cleanDecision != null) dict.Add("cleanDecision", cleanDecision);
            if (todoalarm1 != null) dict.Add("todoalarm1", todoalarm1);
            if (trashTutorial_Start != null) dict.Add("trashTutorial_Start", trashTutorial_Start);
            if (trashTutorial_Fail != null) dict.Add("trashTutorial_Fail", trashTutorial_Fail);
            if (trashTutorial_FindCan != null) dict.Add("trashTutorial_FindCan", trashTutorial_FindCan);
            if (trashTutorial_CanFound != null) dict.Add("trashTutorial_CanFound", trashTutorial_CanFound);
            if (trashTutorial_Skip != null) dict.Add("trashTutorial_Skip", trashTutorial_Skip);
            if (trashTutorial_End != null) dict.Add("trashTutorial_End", trashTutorial_End);
            if (trashTutorial_ToFreeMove != null) dict.Add("trashTutorial_ToFreeMove", trashTutorial_ToFreeMove);
            if (guitar_found != null) dict.Add("guitar_found", guitar_found);
            if (guitar_found2 != null) dict.Add("guitar_found2", guitar_found2);
            if (usb1_first != null) dict.Add("usb1_first", usb1_first);
            if (usb2_first != null) dict.Add("usb2_first", usb2_first);
            if (usb3_first != null) dict.Add("usb3_first", usb3_first);
            if (laptop_open != null) dict.Add("laptop_open", laptop_open);
            if (laptop_need_usb != null) dict.Add("laptop_need_usb", laptop_need_usb);
            if (guitar_case_found != null) dict.Add("guitar_case_found", guitar_case_found);
            if (guitar_case_found2 != null) dict.Add("guitar_case_found2", guitar_case_found2);
            if (guitar_peak_found != null) dict.Add("guitar_peak_found", guitar_peak_found);
            if (guitar_peak_found2 != null) dict.Add("guitar_peak_found2", guitar_peak_found2);
            if (paper_done != null) dict.Add("paper_done", paper_done);
            if (music1 != null) dict.Add("music1", music1);
            if (music2 != null) dict.Add("music2", music2);
            if (music3 != null) dict.Add("music3", music3);
            if (ending_AllFound != null) dict.Add("ending_AllFound", ending_AllFound);
            if (ending_Tune != null) dict.Add("ending_Tune", ending_Tune);
            if (ending_Play1 != null) dict.Add("ending_Play1", ending_Play1);
            if (ending_Play2 != null) dict.Add("ending_Play2", ending_Play2);
            if (ending_Play3 != null) dict.Add("ending_Play3", ending_Play3);
            if (ending_Realization != null) dict.Add("ending_Realization", ending_Realization);
            if (ending_Final != null) dict.Add("ending_Final", ending_Final);
            return dict;
        }
    }

    // 선택지 데이터 클래스 (JSON 파싱용)
    [System.Serializable]
    public class ChoiceData
    {
        public string type; // "choice"
        public List<string> options; // 버튼에 표시될 텍스트 목록
        public List<string> nextKeys; // 각 선택지에 따른 다음 Mono 키 (선택적)
    }

} // Chapter2Manager 클래스 끝