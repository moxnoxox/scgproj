using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatRoomButtonManager : MonoBehaviour
{
    [Header("실제 채팅방 버튼들")]
    public List<Button> roomButtons;   // JSON 기반 방 버튼들

    [Header("더미 채팅방 버튼들")]
    public List<Button> dummyButtons;  // JSON과 무관한 더미 버튼들

    [Header("데이터 로더")]
    public ChatRoomLoader loader; // JSON 받아옴

    [Header("앱 매니저")]
    public ChatAppManager chatAppManager; // 채팅방 열 때 호출

    [Header("새 메시지 표시 (Unread Dot)")]
    public List<GameObject> unreadDots; // RoomButton1~6의 UnreadDot 오브젝트들

    private List<ChatRoom> loadedRooms;

    private void Start()
    {
        loadedRooms = loader.loadedRooms;

        // 실제 방 버튼 연결
        for (int i = 0; i < roomButtons.Count; i++)
        {
            int index = i; // 람다 캡처 방지
            roomButtons[i].onClick.AddListener(() =>
            {
                if (index < loadedRooms.Count)
                {
                    // 🔹 채팅방 열기
                    chatAppManager.OpenChatRoomWithData(loadedRooms[index]);

                    // 🔹 메시지 읽음 처리
                    foreach (var msg in loadedRooms[index].messages)
                    {
                        if (msg.sender != "Me")
                            msg.isRead = true;
                    }

                    // 🔹 배지 갱신
                    UpdateUnreadDots();
                }
            });
        }

        // 더미 버튼 연결
        foreach (var dummy in dummyButtons)
        {
            dummy.onClick.AddListener(() =>
            {
                MonologueManager.Instance.ShowMonologuesSequentially(
                    new List<string> { "여긴 별 볼일 없어..." },
                    3f
                );
            });
        }

        // 시작 시 초기 갱신
        UpdateUnreadDots();
    }

    // 🔹 점 표시 갱신 함수
    public void UpdateUnreadDots()
    {
        if (loadedRooms == null || unreadDots == null) return;

        for (int i = 0; i < loadedRooms.Count && i < unreadDots.Count; i++)
        {
            var room = loadedRooms[i];

            // ✅ 1. 아직 데이터를 불러오지 않은 방 (messages 비어 있음) → 자동으로 배지 켜기
            bool notLoadedYet = room.messages == null || room.messages.Count == 0;

            // ✅ 2. 불러온 상태라면 실제 UnreadCount로 판정
            bool hasUnread = room.UnreadCount > 0;

            // ✅ 3. 두 조건 중 하나라도 true면 배지 켜기
            unreadDots[i].SetActive(notLoadedYet || hasUnread);
        }
    }

}
