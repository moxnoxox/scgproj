using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Linq;

public class ChatManager : MonoBehaviour
{
    public List<ChatRoom> chatRooms = new List<ChatRoom>();
    [HideInInspector] public GameClock gameClock; // 코드로 받아옴

    public RectTransform content; 
    public Button backButton; 
    public TextMeshProUGUI RoomName; 
    public ScrollRect scrollRect;           
    public GameObject myMessagePrefab;       
    public GameObject otherMessagePrefab;   
    public GameObject datePrefab; 
    public GameObject newMessageAlert;

    private bool autoScrollAllowed = true;

    [Header("Answer Choice UI")]
    public RectTransform inputArea;         
    public Button inputBarButton;          
    public GameObject choicePanel;           
    public Transform choicesContainer;       
    public GameObject choiceButtonPrefab; 
    [Range(0, 600f)] public float inputRaiseY = 300f; // 위로 올릴 거리
    public float animTime = 0.25f;
    
    private string lastSender = "";
    private string lastTime = "";

    // 마지막 메시지 UI 저장
    private MyMessageUI lastMyMessageUI;
    private OtherMessageUI lastOtherMessageUI;

    private ChatRoom currentRoom;
    private ChatAppManager appManager;

    private Vector2 inputAreaDefaultPos;
    private VerticalLayoutGroup contentLayout;

    // 현재 방에서 보여줄 선택지
    private List<ChoiceData> pendingChoices;
    private GameObject Player;
    private player_power playerPower;

    private void Awake()
    {
        if (gameClock == null)
            gameClock = GameClock.Instance;

        appManager = FindObjectOfType<ChatAppManager>();

        if (backButton != null && appManager != null)
            backButton.onClick.AddListener(appManager.BackToList);

        // InputArea 기본 위치 저장
        if (inputArea != null)
            inputAreaDefaultPos = inputArea.anchoredPosition;

        if (content != null)
            contentLayout = content.GetComponent<VerticalLayoutGroup>();

        if (choicePanel != null)
            choicePanel.SetActive(false);
        Player = GameObject.FindWithTag("Player");
        if (Player != null)
            playerPower = Player.GetComponent<player_power>();
    }

    private IEnumerator DisableAutoScrollNextFrame()
    {
        yield return null;
        autoScrollAllowed = false;
    }

    public ChatRoom GetCurrentRoom()
    {
        return currentRoom;
    }

    public void SetCurrentRoom(ChatRoom room)
    {
        Debug.Log($"SetCurrentRoom 호출됨: {room.roomName}, " +
                $"initialMessages={room.initialMessages.Count}, messages={room.messages.Count}");

        currentRoom = room;
        pendingChoices = null; // 초기화

        if (RoomName != null)
        {
            RoomName.text = room.roomName;
        }
        
        ClearAllMessages();

        //  처음 열었을 때 initialMessages 
        if (currentRoom.messages.Count == 0 && currentRoom.initialMessages.Count > 0)
        {
            currentRoom.messages.AddRange(currentRoom.initialMessages);
            Debug.Log($"초기 메시지 복사 완료: {currentRoom.messages.Count}개");
        }

        // 현재 메시지 리스트 출력
        foreach (var msg in currentRoom.messages)
        {
            if (msg.type == "dateDivider")
            {
                AddDateDivider(save: false);
                continue;
            }

            if (msg.type == "message")
            {
                if (msg.sender == "Me")
                    AddMyMessage(msg.content, msg.timestamp, autoTime:false, save:false);
                else
                {
                    User senderUser = currentRoom.participants.Find(u => u.id == msg.sender);
                    string senderName = senderUser != null ? senderUser.nickname : msg.sender;
                    Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;

                    AddOtherMessage(senderName, senderProfile, msg.content, msg.timestamp, autoTime:false, save:false, format:"text");
                }
            }
            else if (msg.type == "image")
            {
                User senderUser = currentRoom.participants.Find(u => u.id == msg.sender);
                string senderName = senderUser != null ? senderUser.nickname : msg.sender;
                Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;

                AddOtherMessage(senderName, senderProfile, msg.content, msg.timestamp, autoTime:false, save:false, format:"image");
            }

            if (msg.type == "choice" && !msg.isConsumed)
            {
                pendingChoices = msg.choices;
            }
        }
    

        // 할일 퀘스트 끝나고 
        if (!string.IsNullOrEmpty(currentRoom.AfterQuestJson))
        {
            TextAsset jsonFile = Resources.Load<TextAsset>($"ChatData/{currentRoom.AfterQuestJson}");
            if (jsonFile != null)
            {
                ChatRoom questData = JsonUtility.FromJson<ChatRoom>(jsonFile.text);

                // 자동 메시지들을 순차 출력
                StartCoroutine(PlayAutoMessages(questData.messages));

                Debug.Log($"✅ 퀘스트 JSON {currentRoom.AfterQuestJson} 불러옴");

                currentRoom.AfterQuestJson = null; // 한 번만 실행
            }
        }


        autoScrollAllowed = true;
        StartCoroutine(ScrollToBottomNextFrame());

        // 한 프레임 뒤 autoScrollAllowed 끄기
        StartCoroutine(DisableAutoScrollNextFrame());
    }

    private void ShowChoices(List<ChoiceData> choices)
    {
        if (choices == null || choices.Count == 0) return;

        pendingChoices = choices;

        choicePanel.SetActive(true);
        PopulateChoices(choices);
    }


    public void SendMessage(ChatRoom room, string sender, string content)
    {
        string currentTime = gameClock.GetTimeString();
        Message newMsg = new Message(sender, content, currentTime);
        room.messages.Add(newMsg);

        Debug.Log(sender + ": " + content + " (" + newMsg.timestamp + ")");
    }

    private void AddMessageCommon()
    {
        if (autoScrollAllowed || IsAtBottom())
            StartCoroutine(ScrollToBottomNextFrame());
        else
            ShowNewMessageAlert();
    }

    // --- 내 메시지 ---
    public void AddMyMessage(string text, string time = "", bool autoTime = true, bool save = true)
    {
        Debug.Log($"메시지 추가됨: {text}");

        string finalTime = autoTime ? gameClock.GetTimeString() : time;
        bool sameTime = (lastSender == "Me" && lastTime == finalTime);

        var obj = Instantiate(myMessagePrefab, content);
        var ui = obj.GetComponent<MyMessageUI>();

        // 직전 메시지가 같은 시간대라면 → 직전 메시지 시간 숨기기
        if (sameTime && lastMyMessageUI != null)
            lastMyMessageUI.SetTimeVisible(false);

        // 새 메시지는 항상 시간 표시
        ui.Setup(text, finalTime, autoTime, true);

        // 상태 갱신
        lastSender = "Me";
        lastTime = finalTime;
        lastMyMessageUI = ui;

        if (save && currentRoom != null)
        {
            Message newMsg = new Message("Me", text, finalTime);
            newMsg.isRead = true;
            currentRoom.messages.Add(newMsg);
        }

        AddMessageCommon();
    }

    // --- 상대 메시지 ---
    public void AddOtherMessage(string sender, Sprite profile, string text, string time = "", bool autoTime = true, bool save = true, string format = "text")
    {
        Debug.Log($"[AddOtherMessage] sender={sender}, format={format}, text={text}");

        string finalTime = autoTime ? gameClock.GetTimeString() : time;
        bool sameSender = (lastSender == sender);
        bool sameTime = (lastTime == finalTime);

        var obj = Instantiate(otherMessagePrefab, content);
        var ui = obj.GetComponent<OtherMessageUI>();

        // 같은 사람 & 같은 시간대라면 → 직전 메시지 시간 숨기기
        if (sameSender && sameTime && lastOtherMessageUI != null)
            lastOtherMessageUI.SetTimeVisible(false);

        // 프로필/이름은 첫 메시지일 때만
        bool showProfile = !sameSender;
        bool showName = !sameSender;

        if (format == "image")
        {
            ui.SetupImage(sender, profile, text, finalTime, showProfile, showName, true, autoTime);
        }
        else
        {
            ui.SetupText(sender, profile, text, finalTime, showProfile, showName, true, autoTime);
        }

        // 상태 갱신
        lastSender = sender;
        lastTime = finalTime;
        lastOtherMessageUI = ui;

        if (save && currentRoom != null)
        {
            Message newMsg = new Message(sender, text, finalTime);
            currentRoom.messages.Add(newMsg);
        }

        AddMessageCommon();
    }

    public void AddDateDivider(bool save = true)
    {
        var obj = Instantiate(datePrefab, content);
        var ui = obj.GetComponent<DateDividerUI>();
        ui.Setup();  
        
        // 그룹 끊기
        lastSender = "";
        lastTime = "";
        lastMyMessageUI = null;
        lastOtherMessageUI = null;

        if (save && currentRoom != null)
        {
            Message divider = new Message("dateDivider");
            currentRoom.messages.Add(divider);
        }
    }
    
    private bool IsAtBottom()
    {
        return scrollRect.verticalNormalizedPosition <= 0.01f;
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    private void ShowNewMessageAlert()
    {
        if (newMessageAlert != null)
            newMessageAlert.SetActive(true);
    }

    public void OnNewMessageAlertClicked()
    {
        if (newMessageAlert != null)
            newMessageAlert.SetActive(false);

        StartCoroutine(ScrollToBottomNextFrame());
    }

    // === 선택지 입력창 열고/닫기 ===
    private bool isChoiceOpen = false;

    public void OnInputBarClicked()
    {
        if (isChoiceOpen) return;
        isChoiceOpen = true;

        if (pendingChoices != null && pendingChoices.Count > 0)
        {
            ShowChoices(pendingChoices);

            // 채팅 내용 패딩 늘리기
            if (contentLayout != null)
                contentLayout.padding.bottom = Mathf.RoundToInt(inputRaiseY);

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            Vector2 target = inputAreaDefaultPos + new Vector2(0, inputRaiseY);
            StartCoroutine(MoveInputArea(inputArea, target, animTime));

            // 스크롤을 맨 아래로 강제로 고정
            StartCoroutine(ScrollToBottomNextFrame());
        }
        else
        {
            Debug.Log("⚠ 선택지가 없음");
            isChoiceOpen = false;
        }
    }

    public void OnChoiceSelected(ChoiceData choice) 
    {
        // 내 메시지 보내기
        if (!string.IsNullOrEmpty(choice.resultMessage))
        {
            string time = string.IsNullOrEmpty(choice.timestamp) ? gameClock.GetTimeString() : choice.timestamp;
            AddMyMessage(choice.resultMessage, time);
        }

        // 선택지 닫기
        choicePanel.SetActive(false);
        StartCoroutine(MoveInputArea(inputArea, inputAreaDefaultPos, animTime));
        isChoiceOpen = false;

        if (contentLayout != null)
            contentLayout.padding.bottom = 0;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        StartCoroutine(ScrollToBottomNextFrame());

        // 액션 처리
        if (!string.IsNullOrEmpty(choice.action))
        {
            HandleChoiceAction(choice);
        }
        //에너지 소모 처리
        if(choice.energyCost > 0)
        {
            if (playerPower != null)
                playerPower.DecreasePower(choice.energyCost);
        }

        // 상대방 대답 예약
        if (choice.replies != null)
        {
            foreach (var reply in choice.replies)
                ChatAppManager.Instance.ScheduleReply(currentRoom, reply, reply.delayAfter);
        }

        // 이번 선택지는 기록에서 제거
        var choiceMsg = currentRoom.messages.FirstOrDefault(m => m.type == "choice");
        if (choiceMsg != null)
            currentRoom.messages.Remove(choiceMsg);

        pendingChoices = null;
    }


    private void PopulateChoices(List<ChoiceData> options)
    {
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);

        foreach (var choice in options)
        {
            var go = Instantiate(choiceButtonPrefab, choicesContainer);

            // 텍스트 세팅
            var auto = go.GetComponent<AnswerChoiceUI>();
            if (auto != null)
                auto.SetText(choice.text);

            // 클릭 이벤트 등록
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                ChoiceData captured = choice; // 클로저 방지
                btn.onClick.AddListener(() => OnChoiceSelected(captured));
            }
        }
    }

    private void HandleChoiceAction(ChoiceData choice)
    {
        switch (choice.action)
        {
            case "Monologue":
                if (choice.monologueText != null && choice.monologueText.Count > 0)
                    MonologueManager.Instance.ShowMonologuesSequentially(choice.monologueText, 4f);
                break;

            case "ExitWithMonologue":
                StartCoroutine(ExitWithMonologueAfterDelay(choice.monologueText, 3f));
                break;

            default:
                Debug.LogWarning($"알 수 없는 action: {choice.action}");
                break;
        }
        GameManager.Instance.onReplCount();
    }

    private IEnumerator ExitWithMonologueAfterDelay(List<string> lines, float delay)
    {
        if (lines != null && lines.Count > 0)
            MonologueManager.Instance.ShowMonologuesSequentially(lines, 4f);
        yield return new WaitForSeconds(delay);
        appManager.BackToList();
    }

    private IEnumerator PlayAutoMessages(List<Message> autoMessages)
    {
        foreach (var msg in autoMessages)
        {
            float delay = msg.delayAfter > 0 ? msg.delayAfter : 2f;
            yield return new WaitForSeconds(delay);

            if (msg.type == "message")
            {
                if (msg.sender == "Me")
                    AddMyMessage(msg.content, msg.timestamp, autoTime: false, save: true);
                else
                {
                    User senderUser = currentRoom.participants.Find(u => u.id == msg.sender);
                    string senderName = senderUser != null ? senderUser.nickname : msg.sender;
                    Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;
                    AddOtherMessage(senderName, senderProfile, msg.content, msg.timestamp, autoTime: false, save: true, format:"text");
                }
            }
            else if (msg.type == "image") // 🔹 추가
            {
                User senderUser = currentRoom.participants.Find(u => u.id == msg.sender);
                string senderName = senderUser != null ? senderUser.nickname : msg.sender;
                Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;
                AddOtherMessage(senderName, senderProfile, msg.content, msg.timestamp, autoTime:false, save:true, format:"image");
            }
            else if (msg.type == "dateDivider")
            {
                AddDateDivider(save: true);
            }
        }
    }



    // 입력 영역 Y 이동(간단한 코루틴 애니메이션)
    private IEnumerator MoveInputArea(RectTransform target, Vector2 endPos, float duration)
    {
        if (target == null) yield break;

        Vector2 start = target.anchoredPosition;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            target.anchoredPosition = Vector2.Lerp(start, endPos, k);
            yield return null;
        }
        target.anchoredPosition = endPos;
    }

    public void ClearAllMessages()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        lastSender = "";
        lastTime = "";
        lastMyMessageUI = null;
        lastOtherMessageUI = null;
    }
}
