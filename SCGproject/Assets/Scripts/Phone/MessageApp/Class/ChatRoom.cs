using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChatRoom
{
    public string roomName;                  // 채팅방 이름
    public string profileImagePath;          // JSON에서 불러올 이미지 이름
    [HideInInspector] public Sprite profileImage; // 런타임에서 로드됨

    public List<User> participants;          // 방에 참여하는 유저들
    public List<Message> initialMessages = new List<Message>(); // 시나리오용
    [HideInInspector] public List<Message> messages = new List<Message>(); // 런타임 누적 메시지

    public ChatRoom(string roomName)
    {
        this.roomName = roomName;
        participants = new List<User>();
        messages = new List<Message>();
    }

    // 안 읽은 메시지 개수
    public int UnreadCount
    {
        get
        {
            int count = 0;
            foreach (var msg in messages)
            {
                if (!msg.isRead) count++;
            }
            return count;
        }
    }
}
