using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChatAppManager : MonoBehaviour
{
    public static ChatAppManager Instance { get; private set; } // 싱글톤 인스턴스

    [Header("패널들")]
    public GameObject chatListPanel;       // 채팅방 리스트 화면
    public GameObject chatRoomPrefab;      // 채팅방 프리팹 (ChatRoomPanel)
    public Transform ChatAppPanel;         // ChatRoomPanel이 들어갈 부모

    private GameObject currentRoomPanel;   // 현재 열려있는 채팅방 인스턴스
    private ChatManager chatManager;       // 현재 방의 ChatManager

    // === reply 스케줄 관리 ===
    private class ScheduledReply
    {
        public ChatRoom room;
        public ReplyData reply;
        public float triggerTime;
    }

    private List<ScheduledReply> scheduledReplies = new List<ScheduledReply>();

    private void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // 시작할 때 리스트 열기
        OpenChatList();
    }

    private void OnDisable()
    {
        BackInputManager.Unregister(OnBackPressedFromRoom);
    }

    private void Update()
    {
        float now = Time.time;
        for (int i = scheduledReplies.Count - 1; i >= 0; i--)
        {
            if (now >= scheduledReplies[i].triggerTime)
            {
                var item = scheduledReplies[i];

                // 방이 열려있을 때만 UI 표시
                if (chatManager != null && chatManager.GetCurrentRoom() == item.room)
                {
                    // sender 매칭
                    string senderId = item.reply.sender.Trim();
                    User senderUser = item.room.participants.Find(u => u.id == senderId);

                    string senderName = senderUser != null ? senderUser.nickname : item.reply.sender;
                    Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;

                    string time = string.IsNullOrEmpty(item.reply.timestamp)
                        ? GameClock.Instance.GetTimeString()
                        : item.reply.timestamp;

                    // UI에 출력 (nickname + 프로필)
                    chatManager.AddOtherMessage(senderName, senderProfile, item.reply.content, time, autoTime:false, save:false);
                }

                // 기록 남김
                string recordTime = string.IsNullOrEmpty(item.reply.timestamp)
                    ? GameClock.Instance.GetTimeString()
                    : item.reply.timestamp;

                Message newMsg = new Message(item.reply.sender, item.reply.content, recordTime);
                item.room.messages.Add(newMsg);

                scheduledReplies.RemoveAt(i);
            }
        }
}


    public void ScheduleReply(ChatRoom room, ReplyData reply, float delay)
    {
        scheduledReplies.Add(new ScheduledReply
        {
            room = room,
            reply = reply,
            triggerTime = Time.time + delay
        });
    }

    public void OpenChatList()
    {
        chatListPanel.SetActive(true);

        if (currentRoomPanel != null)
        {
            Destroy(currentRoomPanel);
            currentRoomPanel = null;
            chatManager = null;
        }
    }

    public void OpenChatRoomWithData(ChatRoom roomData)
    {
        chatListPanel.SetActive(false);

        if (currentRoomPanel != null)
            Destroy(currentRoomPanel);

        currentRoomPanel = Instantiate(chatRoomPrefab, ChatAppPanel);
        chatManager = currentRoomPanel.GetComponent<ChatManager>();

        if (chatManager != null)
            chatManager.SetCurrentRoom(roomData);

        currentRoomPanel.SetActive(true);

        BackInputManager.Register(OnBackPressedFromRoom);
    }

    public void BackToList()
    {
        CloseRoom();
    }

    private void OnBackPressedFromRoom()
    {
        CloseRoom();
        BackInputManager.Unregister(OnBackPressedFromRoom);
    }

    private void CloseRoom()
    {
        if (currentRoomPanel != null)
        {
            Destroy(currentRoomPanel);
            currentRoomPanel = null;
            chatManager = null;
            chatListPanel.SetActive(true);
        }
    }
}
