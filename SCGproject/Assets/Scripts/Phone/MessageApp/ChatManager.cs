using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public List<ChatRoom> chatRooms = new List<ChatRoom>();
    [HideInInspector] public GameClock gameClock; // 코드로 받아옴

    public Transform content;               
    public GameObject myMessagePrefab;       
    public GameObject otherMessagePrefab;    

    private string lastSender = "";
    private string lastTime = "";

    // 마지막 메시지 UI 저장
    private MyMessageUI lastMyMessageUI;
    private OtherMessageUI lastOtherMessageUI;

    private ChatRoom currentRoom;

    private void Awake()
    {
        if (gameClock == null)
            gameClock = GameClock.Instance;
    }

    private void Start()
    {
    
        // 작성 예시 
        // 1) 실시간 메시지 (자동 시간) 
            // chatManager.AddMyMessage("방금 보낸 메시지"); 

        // 2) 히스토리 메시지 (수동 시간) 
            // chatManager.AddMyMessage("어제 받은 메시지", "오전 9:00", autoTime:false);
    }

    public void SetCurrentRoom(ChatRoom room)
    {
        Debug.Log($"SetCurrentRoom 호출됨: {room.roomName}, " +
                $"initialMessages={room.initialMessages.Count}, messages={room.messages.Count}");

        currentRoom = room;
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


    public void SendMessage(ChatRoom room, string sender, string content)
    {
        string currentTime = gameClock.GetTimeString();
        Message newMsg = new Message(sender, content, currentTime);
        room.messages.Add(newMsg);

        Debug.Log(sender + ": " + content + " (" + newMsg.timestamp + ")");
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



