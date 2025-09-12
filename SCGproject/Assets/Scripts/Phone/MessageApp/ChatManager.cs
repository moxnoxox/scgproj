using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class ChatManager : MonoBehaviour
{
    public List<ChatRoom> chatRooms = new List<ChatRoom>();
    [HideInInspector] public GameClock gameClock; // 코드로 받아옴

    public Transform content; 
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
    }

    private IEnumerator DisableAutoScrollNextFrame()
    {
        yield return null;
        autoScrollAllowed = false;
    }

    public void SetCurrentRoom(ChatRoom room)
    {
        Debug.Log($"SetCurrentRoom 호출됨: {room.roomName}, " +
                $"initialMessages={room.initialMessages.Count}, messages={room.messages.Count}");

        currentRoom = room;
        

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
                {
                    Debug.Log($"내 메시지 출력: {msg.content}");
                    AddMyMessage(msg.content, msg.timestamp, autoTime:false, save:false);
                }
                else
                {
                    User senderUser = currentRoom.participants.Find(u => u.id == msg.sender);
                    string senderName = senderUser != null ? senderUser.nickname : msg.sender;
                    Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;

                    Debug.Log($"상대 메시지 출력: {senderName} / {msg.content}");
                    AddOtherMessage(senderName, senderProfile, msg.content, msg.timestamp, autoTime:false, save:false);
                }
            }
        }

        autoScrollAllowed = true;
        StartCoroutine(ScrollToBottomNextFrame());

        // 한 프레임 뒤 autoScrollAllowed 끄기
        StartCoroutine(DisableAutoScrollNextFrame());
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
            currentRoom.messages.Add(newMsg);
        }

        AddMessageCommon();
    }

    // --- 상대 메시지 ---
    public void AddOtherMessage(string sender, Sprite profile, string text, string time = "", bool autoTime = true, bool save = true)
    {
        Debug.Log($"메시지 추가됨: {text}");

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

        // 새 메시지는 항상 시간 표시
        ui.Setup(sender, profile, text, finalTime, showProfile, showName, true, autoTime);

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

        choicePanel.SetActive(true);  
        string[] options = { "그래, 알겠어", "지금은 바빠", "나중에 얘기하자" };
        PopulateChoices(options);

        Vector2 target = inputAreaDefaultPos + new Vector2(0, inputRaiseY);
        StartCoroutine(MoveInputArea(inputArea, target, animTime));
    }

    public void OnChoiceSelected(string choiceText)
    {
        // 내 메시지 전송
        AddMyMessage(choiceText);

        // 선택지 닫기
        choicePanel.SetActive(false);
        StartCoroutine(MoveInputArea(inputArea, inputAreaDefaultPos, animTime));
        isChoiceOpen = false;

        StartCoroutine(ScrollToBottomNextFrame());

        // 상대 답변 예시 (2초 후)
        StartCoroutine(ReplyAfterDelay("응, 알겠어", 2f));
    }

    private void PopulateChoices(string[] options)
    {
        // 기존 버튼 제거
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);

        foreach (var text in options)
        {
            // 프리팹에서 새 버튼 생성
            var go = Instantiate(choiceButtonPrefab, choicesContainer) as GameObject;

            // 텍스트/사이즈 조정
            var auto = go.GetComponent<AnswerChoiceUI>();
            if (auto != null)
            {
                auto.SetText(text);  // 내부에서 한 프레임 미뤄 사이즈 계산
            }
            else
            {
                var label = go.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = text;
            }

            // 클릭 이벤트 등록
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                string choice = text;
                btn.onClick.AddListener(() => OnChoiceSelected(choice));
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


    // 상대방 답변 테스트용
    private System.Collections.IEnumerator ReplyAfterDelay(string reply, float delay)
    {
        yield return new WaitForSeconds(delay);
        // 예: mom에게서 옴 
        AddOtherMessage("mom", null, reply);
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



