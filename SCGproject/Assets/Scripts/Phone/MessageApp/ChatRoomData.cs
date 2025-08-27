using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChatRoomData
{
    public string roomName;
    public Sprite profileImage;

    // 시작할 때 보여줄 메시지들 (Inspector나 JSON 등으로 미리 넣어둠)
    public List<Message> initialMessages = new List<Message>();

    // 실제 플레이 도중 쌓이는 메시지
    public List<Message> currentMessages = new List<Message>();
}
