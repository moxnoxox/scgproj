using UnityEngine;

public class FinalChatTrigger : MonoBehaviour
{
    public ChatRoomLoader loader;   
    public string targetRoomName = "🎸";  
    public string questJsonFile = "guitar_afterquest"; // 예: Resources/JSON/guitar_afterquest.json

    public void StartFinalChat()
    {
        ChatRoom targetRoom = loader.loadedRooms
            .Find(r => r.roomName == targetRoomName);

        if (targetRoom == null)
        {
            Debug.LogError($"❌ 방을 찾을 수 없음: {targetRoomName}");
            return;
        }

        Message divider = new Message("dateDivider");
        targetRoom.messages.Add(divider);

        // 트리거 메시지 추가
        Message msg1 = new Message("guitar", "야 혹시", "오후 6:02");
        msg1.isRead = false;

        Message msg2 = new Message("guitar", "너네 집에 있는 내 기타 좀 찾아줄 수 있어?", "오후 6:02");
        msg2.isRead = false;

        targetRoom.messages.Add(msg1);
        targetRoom.messages.Add(msg2);

        // 알림 띄우기 추가해야함@@
        /// PhoneUIManager.Instance.ShowNotification(targetRoom.roomName, msg1.content);

         targetRoom.AfterQuestJson = questJsonFile;

        Debug.Log($"퀘스트 트리거 완료: {targetRoomName} 방에 메시지 2개 추가 + {questJsonFile} 로드");
    }
}

