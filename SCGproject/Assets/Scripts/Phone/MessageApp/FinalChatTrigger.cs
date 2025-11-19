using UnityEngine;
using System.Collections.Generic;   // ← 리스트 쓸 거면 같이 넣어두자

public class FinalChatTrigger : MonoBehaviour
{
    public ChatRoomLoader loader;   
    public string targetRoomName = "🎸";
    public string questJsonFile = "guitar_afterquest"; // 예: Resources/ChatData/guitar_afterquest.json
    public string nextQuestJsonFile = "guitar_afterquest2";
    public static FinalChatTrigger Instance;
    public bool isChatDone = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartFinalChat()
    {
       isChatDone = false;

        // PhoneDataManager에서 지속된 ChatRoom 가져오기
        ChatRoom targetRoom = PhoneDataManager.Instance.chatRooms
            .Find(r => r.roomName == targetRoomName);

        if (targetRoom == null)
        {
            Debug.LogError($"❌ FinalChatTrigger: '{targetRoomName}' 채팅방을 찾지 못했습니다.");
            return;
        }

        // ----- 여기부터는 기존 로직 그대로 -----
        Message divider = new Message("dateDivider");
        targetRoom.messages.Add(divider);

        // 트리거 메시지 추가
        Message msg1 = new Message("guitar", "야", "오후 6:01");
        msg1.isRead = false;

        Message msg2 = new Message("guitar", "야아아아ㅏ", "오후 6:01");
        msg2.isRead = false;

        Message msg3 = new Message("guitar", "ㅑㅑㅑ", "오후 6:01");
        msg3.isRead = false;

        targetRoom.messages.Add(msg1);
        targetRoom.messages.Add(msg2);
        targetRoom.messages.Add(msg3);

        var manager = ChatAppManager.Instance?.GetComponentInChildren<ChatRoomButtonManager>(true);
        manager?.UpdateUnreadDots();

        // 알림 띄우기 추가해야함@@
        // PhoneUIManager.Instance.ShowNotification(targetRoom.roomName, msg1.content);

        targetRoom.AfterQuestJson = questJsonFile;
        targetRoom.AfterQuestJsonNext = nextQuestJsonFile;

        Debug.Log($"퀘스트 트리거 완료: {targetRoomName} 방에 메시지 3개 추가 + {questJsonFile} 세팅");


    }
}
