using UnityEngine;

public class FinalChatTrigger : MonoBehaviour
{
    public ChatRoomLoader loader;   
    public string targetRoomName = "🎸";
    public string questJsonFile = "guitar_afterquest"; // 예: Resources/JSON/guitar_afterquest.json
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
        Message msg1 = new Message("guitar", "야", "오후 6:01");
        msg1.isRead = false;

        Message msg2 = new Message("guitar", "야아아아ㅏ", "오후 6:01");
        msg2.isRead = false;

        Message msg3 = new Message("guitar", "ㅑㅑㅑ", "오후 6:01");
        msg3.isRead = false;

        targetRoom.messages.Add(msg1);
        targetRoom.messages.Add(msg2);
        targetRoom.messages.Add(msg3);

        // 알림 띄우기 추가해야함@@
        /// PhoneUIManager.Instance.ShowNotification(targetRoom.roomName, msg1.content);

        targetRoom.AfterQuestJson = questJsonFile;
        targetRoom.AfterQuestJsonNext = nextQuestJsonFile; ;

        Debug.Log($"퀘스트 트리거 완료: {targetRoomName} 방에 메시지 3개 추가 + {questJsonFile} 로드");
    }
}

