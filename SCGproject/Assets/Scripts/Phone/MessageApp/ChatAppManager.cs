using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChatAppManager : MonoBehaviour
{
    public static ChatAppManager Instance { get; private set; }

    [Header("패널들")]
    public GameObject chatListPanel;
    public GameObject chatRoomPrefab;
    public Transform ChatAppPanel;

    private GameObject currentRoomPanel;
    private ChatManager chatManager;

    private class ScheduledReply
    {
        public ChatRoom room;
        public ReplyData reply;
        public float triggerTime;
    }

    private List<ScheduledReply> scheduledReplies = new List<ScheduledReply>();

    private void Awake()
    {
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

                if (chatManager != null && chatManager.GetCurrentRoom() == item.room)
                {
                    string senderId = item.reply.sender.Trim();
                    User senderUser = item.room.participants.Find(u => u.id == senderId);

                    string senderName = senderUser != null ? senderUser.nickname : item.reply.sender;
                    Sprite senderProfile = senderUser != null ? senderUser.profileImage : null;

                    string time = string.IsNullOrEmpty(item.reply.timestamp)
                        ? GameClock.Instance.GetTimeString()
                        : item.reply.timestamp;

                    chatManager.AddOtherMessage(senderName, senderProfile, item.reply.content, time, autoTime: false, save: false);
                }

                string recordTime = string.IsNullOrEmpty(item.reply.timestamp)
                    ? GameClock.Instance.GetTimeString()
                    : item.reply.timestamp;

                Message newMsg = new Message(item.reply.sender, item.reply.content, recordTime);
                item.room.messages.Add(newMsg);

                FindObjectOfType<ChatRoomButtonManager>()?.UpdateUnreadDots();

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
        // 🔹 자동 대화 중엔 방 이동 불가
        if (chatManager != null && chatManager.IsAutoPlaying)
        {
            Debug.Log("⚠ 자동 대화 중엔 다른 방으로 이동할 수 없습니다.");
            return;
        }

        chatListPanel.SetActive(false);

        // 🔹 같은 방이면 새로 만들지 않고 이어서 사용
        if (chatManager != null && chatManager.GetCurrentRoom() == roomData)
        {
            Debug.Log("✅ 이미 같은 방이 열려 있음 — 새로 만들지 않음");
            currentRoomPanel.SetActive(true);
            chatManager.SetCurrentRoom(roomData); // 이어붙이기 포함
            return;
        }

        // 🔹 다른 방일 때만 새로 생성
        if (currentRoomPanel != null)
            Destroy(currentRoomPanel);

        currentRoomPanel = Instantiate(chatRoomPrefab, ChatAppPanel);
        chatManager = currentRoomPanel.GetComponent<ChatManager>();

        if (chatManager != null)
            chatManager.SetCurrentRoom(roomData);

        // ✅ 방 들어올 때 읽음 처리
        foreach (var msg in roomData.messages)
        {
            if (msg.sender != "Me")
                msg.isRead = true;
        }

        // ✅ 버튼 배지 갱신
        FindObjectOfType<ChatRoomButtonManager>()?.UpdateUnreadDots();

        currentRoomPanel.SetActive(true);
        BackInputManager.Register(OnBackPressedFromRoom);
    }

    public void BackToList()
    {
        if (chatManager != null && chatManager.IsAutoPlaying)
        {
            Debug.Log("⚠ 자동 대화 중엔 나갈 수 없습니다.");
            return;
        }

        CloseRoom();
    }

    private void OnBackPressedFromRoom()
    {
        if (chatManager != null && chatManager.IsAutoPlaying)
        {
            Debug.Log("⚠ 자동 대화 중엔 나갈 수 없습니다.");
            return;
        }

        CloseRoom();
        BackInputManager.Unregister(OnBackPressedFromRoom);
    }

    private void CloseRoom()
    {
        InputBlocker.Disable();

        if (currentRoomPanel != null)
        {
            Destroy(currentRoomPanel);
            currentRoomPanel = null;
            chatManager = null;
        }

        chatListPanel.SetActive(true);
        FindObjectOfType<ChatRoomButtonManager>()?.UpdateUnreadDots();
    }
}
