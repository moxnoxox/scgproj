using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ChatManager : MonoBehaviour
{
    public List<ChatRoom> chatRooms = new List<ChatRoom>();
    [HideInInspector] public GameClock gameClock;

    [Header("Core UI")]
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
    [Range(0, 600f)] public float inputRaiseY = 300f;
    public float animTime = 0.25f;

    private string lastSender = "";
    private string lastTime = "";

    private MyMessageUI lastMyMessageUI;
    private OtherMessageUI lastOtherMessageUI;

    private ChatRoom currentRoom;
    private ChatAppManager appManager;

    private Vector2 inputAreaDefaultPos;
    private VerticalLayoutGroup contentLayout;

    private bool isAutoPlaying = false;
    public bool IsAutoPlaying => isAutoPlaying;

    private List<ChoiceData> pendingChoices;
    private bool isChoiceOpen = false;

    private GameObject Player;
    private player_power playerPower;

    private static HashSet<string> loadedQuestKeys = new HashSet<string>();
    public static void ResetStatics()
    {
        loadedQuestKeys.Clear();
    }

    private void Awake()
    {
        if (gameClock == null)
            gameClock = GameClock.Instance;

        appManager = FindObjectOfType<ChatAppManager>();

        if (backButton != null && appManager != null)
            backButton.onClick.AddListener(appManager.BackToList);

        if (inputArea != null)
            inputAreaDefaultPos = inputArea.anchoredPosition;

        if (content != null)
            contentLayout = content.GetComponent<VerticalLayoutGroup>();

        if (choicePanel != null)
            choicePanel.SetActive(false);

        Player = GameObject.FindWithTag("Player");
        if (Player != null)
            playerPower = Player.GetComponent<player_power>();

        if (scrollRect != null)
            scrollRect.onValueChanged.AddListener(_ => CheckScrollPosition());
    }

    private IEnumerator DisableAutoScrollNextFrame()
    {
        yield return null;
        autoScrollAllowed = false;
    }

    public ChatRoom GetCurrentRoom() => currentRoom;

    // ===== 채팅방 세팅 =====
    public void SetCurrentRoom(ChatRoom room)
    {
        Debug.Log($"SetCurrentRoom 호출됨: {room.roomName}");

        // 🔹 이미 같은 방이면 다시 초기화하지 않음
        if (currentRoom == room)
        {
            Debug.Log("같은 방 재설정 방지 → 무시");
            return;
        }

        // 🔹 다른 방일 때만 새로 초기화
        currentRoom = room;
        pendingChoices = null;
        ClearAllMessages();

        if (RoomName != null)
            RoomName.text = room.roomName;

        if (currentRoom.messages.Count == 0 && currentRoom.initialMessages.Count > 0)
        {
            currentRoom.messages.AddRange(currentRoom.initialMessages);
            Debug.Log($"초기 메시지 복사 완료: {currentRoom.messages.Count}개");
        }

        // 기존 메시지 출력
        foreach (var msg in currentRoom.messages)
        {
            if (msg.type == "dateDivider")
                AddDateDivider(false);
            else if (msg.type == "message")
            {
                if (msg.sender == "Me")
                    AddMyMessage(msg.content, msg.timestamp, false, false);
                else
                {
                    User senderUser = currentRoom.participants.Find(u => u.id == msg.sender);
                    string senderName = senderUser != null ? senderUser.nickname : msg.sender;
                    Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;
                    AddOtherMessage(senderName, senderProfile, msg.content, msg.timestamp, false, false, "text");
                }
            }
            else if (msg.type == "image")
            {
                User senderUser = currentRoom.participants.Find(u => u.id == msg.sender);
                string senderName = senderUser != null ? senderUser.nickname : msg.sender;
                Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;
                AddOtherMessage(senderName, senderProfile, msg.content, msg.timestamp, false, false, "image");
            }

            if (msg.type == "choice" && !msg.isConsumed)
                pendingChoices = msg.choices;
        }

        // AfterQuestJson 이어붙이기    
        if (!string.IsNullOrEmpty(currentRoom.AfterQuestJson))
        {
            string key = currentRoom.roomName + "|" + currentRoom.AfterQuestJson;

            // 이미 로드한 JSON이면 스킵
            if (loadedQuestKeys.Contains(key))
            {
                Debug.Log($"⚠ {key} 는 이미 한번 붙여져 있어서 스킵함.");
            }
            else
            {
                string path = $"ChatData/{currentRoom.AfterQuestJson}";
                TextAsset jsonFile = Resources.Load<TextAsset>(path);

                if (jsonFile != null)
                {
                    ChatRoom questData = JsonUtility.FromJson<ChatRoom>(jsonFile.text);

                    if (questData != null && questData.messages != null)
                    {
                        int before = currentRoom.messages.Count;

                        currentRoom.messages.AddRange(questData.messages);
                        var newMsgs = currentRoom.messages.Skip(before).ToList();
                        StartCoroutine(PlayAutoMessages(newMsgs, alreadySaved: true));

                        Debug.Log($"📥 {currentRoom.AfterQuestJson} 이어붙임 ({before}→{currentRoom.messages.Count})");

                        loadedQuestKeys.Add(key);
                    }
                }
                else
                {
                    Debug.LogError($"❌ AfterQuestJson 로드 실패: {path}");
                }
            }

            // 다음엔 붙지 않도록 비워둠
            currentRoom.AfterQuestJson = null;
        }

        autoScrollAllowed = true;
        StartCoroutine(ScrollToBottomNextFrame());
        StartCoroutine(DisableAutoScrollNextFrame());
    }

    // ===== 선택지 =====
    private void ShowChoices(List<ChoiceData> choices)
    {
        if (choices == null || choices.Count == 0) return;

        pendingChoices = choices;
        choicePanel.SetActive(true);
        PopulateChoices(choices);
    }

    public void OnInputBarClicked()
    {
        if (isChoiceOpen) return;
        isChoiceOpen = true;

        if (pendingChoices != null && pendingChoices.Count > 0)
        {
            ShowChoices(pendingChoices);
            if (contentLayout != null)
                contentLayout.padding.bottom = Mathf.RoundToInt(inputRaiseY);

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            Vector2 target = inputAreaDefaultPos + new Vector2(0, inputRaiseY);
            StartCoroutine(MoveInputArea(inputArea, target, animTime));

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
        if (!string.IsNullOrEmpty(choice.resultMessage))
        {
            string time = string.IsNullOrEmpty(choice.timestamp) ? gameClock.GetTimeString() : choice.timestamp;
            AddMyMessage(choice.resultMessage, time, autoTime: false);
        }

        choicePanel.SetActive(false);
        StartCoroutine(MoveInputArea(inputArea, inputAreaDefaultPos, animTime));
        isChoiceOpen = false;

        if (contentLayout != null)
            contentLayout.padding.bottom = 0;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        StartCoroutine(ScrollToBottomNextFrame());

        if (!string.IsNullOrEmpty(choice.action))
            HandleChoiceAction(choice);

        if (choice.energyCost > 0 && playerPower != null)
            playerPower.DecreasePower(choice.energyCost);

        if (choice.replies != null)
        {
            foreach (var reply in choice.replies)
                ChatAppManager.Instance.ScheduleReply(currentRoom, reply, reply.delayAfter);
        }

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

            var auto = go.GetComponent<AnswerChoiceUI>();
            if (auto != null)
                auto.SetText(choice.text);

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                ChoiceData captured = choice;
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

    // ===== 메시지 =====
    private void AddMessageCommon()
    {
        if (autoScrollAllowed || IsAtBottom())
            StartCoroutine(ScrollToBottomNextFrame());
        else
            ShowNewMessageAlert();
    }

    public void AddMyMessage(string text, string time = "", bool autoTime = true, bool save = true)
    {
        string finalTime = autoTime ? gameClock.GetTimeString() : time;
        bool sameTime = (lastSender == "Me" && lastTime == finalTime);

        var obj = Instantiate(myMessagePrefab, content);
        var ui = obj.GetComponent<MyMessageUI>();

        if (sameTime && lastMyMessageUI != null)
            lastMyMessageUI.SetTimeVisible(false);

        ui.Setup(text, finalTime, autoTime, true);

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

    public void AddOtherMessage(string sender, Sprite profile, string text,
        string time = "", bool autoTime = true, bool save = true, string format = "text")
    {
        string finalTime = autoTime ? gameClock.GetTimeString() : time;
        bool sameSender = (lastSender == sender);
        bool sameTime = (lastTime == finalTime);

        var obj = Instantiate(otherMessagePrefab, content);
        var ui = obj.GetComponent<OtherMessageUI>();

        if (sameSender && sameTime && lastOtherMessageUI != null)
            lastOtherMessageUI.SetTimeVisible(false);

        bool showProfile = !sameSender;
        bool showName = !sameSender;

        if (format == "image")
            ui.SetupImage(sender, profile, text, finalTime, showProfile, showName, true, autoTime);
        else
            ui.SetupText(sender, profile, text, finalTime, showProfile, showName, true, autoTime);

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
        obj.GetComponent<DateDividerUI>().Setup();

        lastSender = "";
        lastTime = "";
        lastMyMessageUI = null;
        lastOtherMessageUI = null;

        if (save && currentRoom != null)
            currentRoom.messages.Add(new Message("dateDivider"));
    }

    private bool IsAtBottom() => scrollRect.verticalNormalizedPosition <= 0.01f;

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;

        // ✅ 현재 채팅창이 활성화되어 있을 때만 실행
        if (scrollRect != null && scrollRect.isActiveAndEnabled && scrollRect.gameObject.activeInHierarchy)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }

    private void CheckScrollPosition()
    {
        if (IsAtBottom() && newMessageAlert != null && newMessageAlert.activeSelf)
            newMessageAlert.SetActive(false);
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

    // ===== 자동 대화 (InputBlocker 기반) =====
    public IEnumerator PlayAutoMessages(List<Message> autoMessages, bool alreadySaved = false)
    {
        isAutoPlaying = true;

        InputBlocker.Enable(); // 전역 입력 차단

        foreach (var msg in new List<Message>(autoMessages))
        {
            float delay = msg.delayAfter > 0 ? msg.delayAfter : 2f;
            yield return new WaitForSeconds(delay);

            if (msg.type == "message")
            {
                if (msg.sender == "Me")
                    AddMyMessage(msg.content, msg.timestamp, autoTime: false, save: !alreadySaved);
                else
                {
                    User senderUser = currentRoom.participants.Find(u => u.id == msg.sender);
                    string senderName = senderUser != null ? senderUser.nickname : msg.sender;
                    Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;

                    AddOtherMessage(
                        senderName,
                        senderProfile,
                        msg.content,
                        msg.timestamp,
                        autoTime: false,
                        save: !alreadySaved,
                        format: "text");
                }
            }
            else if (msg.type == "image")
            {
                User senderUser = currentRoom.participants.Find(u => u.id == msg.sender);
                string senderName = senderUser != null ? senderUser.nickname : msg.sender;
                Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;

                AddOtherMessage(
                    senderName,
                    senderProfile,
                    msg.content,
                    msg.timestamp,
                    autoTime: false,
                    save: !alreadySaved,
                    format: "image");
            }
            else if (msg.type == "dateDivider")
            {
                AddDateDivider(save: !alreadySaved);
            }

            StartCoroutine(ScrollToBottomNextFrame());
        }

        InputBlocker.Disable(); // 입력 복구
        isAutoPlaying = false;

        if (currentRoom != null && currentRoom.roomName == "<sprite name=emoji_guitar>" && FinalChatTrigger.Instance != null)
        {
            FinalChatTrigger.Instance.isChatDone = true;
            Debug.Log("FinalChatTrigger: <sprite name=emoji_guitar>방 자동 대화 완료 신호 보냄");
        }
    }

    // ===== 유틸 =====
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
            Destroy(child.gameObject);

        lastSender = "";
        lastTime = "";
        lastMyMessageUI = null;
        lastOtherMessageUI = null;
    }
}
