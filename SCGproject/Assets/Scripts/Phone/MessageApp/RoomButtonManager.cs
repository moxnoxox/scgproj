using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatRoomButtonManager : MonoBehaviour
{
    [Header("채팅방 버튼들 (투명)")]
    public List<Button> roomButtons;   // 방마다 배치한 버튼들

    [Header("데이터 로더")]
    public ChatRoomLoader loader; // JSON 받아옴

    [Header("채팅방 리스트")]
    public List<ChatRoom> chatRooms; 

    [Header("앱 매니저")]
    public ChatAppManager chatAppManager; // 채팅방 열 때 호출

    private void Start()
    {
        List<ChatRoom> chatRooms = loader.loadedRooms;

        // 버튼 개수와 방 개수가 안 맞으면 경고
        if (roomButtons.Count != chatRooms.Count)
        {
            Debug.LogWarning("버튼 개수와 채팅방 개수가 다릅니다.");
        }

        // 버튼 클릭 이벤트 연결
        for (int i = 0; i < roomButtons.Count; i++)
        {
            int index = i; // 람다 캡처 문제 방지
            roomButtons[i].onClick.AddListener(() => chatAppManager.OpenChatRoomWithData(chatRooms[index]));
        }
    }

    private void OnRoomButtonClicked(int index)
    {
        if (index >= 0 && index < chatRooms.Count)
        {
            chatAppManager.OpenChatRoomWithData(chatRooms[index]);
        }
        else
        {
            Debug.LogError($"ChatRoomButtonManager: 잘못된 인덱스 {index}");
        }
    }
}
