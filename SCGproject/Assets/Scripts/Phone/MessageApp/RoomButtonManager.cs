using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatRoomButtonManager : MonoBehaviour
{
    [Header("실제 채팅방 버튼들")]
    public List<Button> roomButtons;

    [Header("더미 채팅방 버튼들")]
    public List<Button> dummyButtons;

    [Header("앱 매니저")]
    public ChatAppManager chatAppManager;

    [Header("새 메시지 표시 (Unread Dot)")]
    public List<GameObject> unreadDots;

    private List<ChatRoom> loadedRooms;

    private void Start()
    {
        // ✔ 챕터1, 챕터2, 챕터3 어디서든 동일하게 데이터 가져오기
        loadedRooms = PhoneDataManager.Instance.chatRooms;

        if (loadedRooms == null || loadedRooms.Count == 0)
        {
            Debug.LogError("ChatRoomButtonManager: PhoneDataManager에 chatRooms가 없습니다.");
            return;
        }

        // 실제 방 버튼 연결
        for (int i = 0; i < roomButtons.Count; i++)
        {
            int index = i;
            roomButtons[i].onClick.AddListener(() =>
            {
                if (index < loadedRooms.Count)
                {
                    ChatRoom room = loadedRooms[index];

                    // 👉 채팅방 열기
                    chatAppManager.OpenChatRoomWithData(room);

                    // 👉 읽음 처리
                    foreach (var msg in room.messages)
                        if (msg.sender != "Me") msg.isRead = true;

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

        // 초기 갱신
        UpdateUnreadDots();
    }

    public void UpdateUnreadDots()
    {
        if (loadedRooms == null || unreadDots == null) return;

        for (int i = 0; i < loadedRooms.Count && i < unreadDots.Count; i++)
        {
            ChatRoom room = loadedRooms[i];

            bool notLoadedYet = room.messages == null || room.messages.Count == 0;
            bool hasUnread = room.UnreadCount > 0;

            unreadDots[i].SetActive(notLoadedYet || hasUnread);
        }
    }

   /* public void UpdateUnreadDots()
{
    if (loadedRooms == null || unreadDots == null) return;

    for (int i = 0; i < unreadDots.Count; i++)
    {
        if (i >= loadedRooms.Count)
        {
            unreadDots[i].SetActive(false);
            continue;
        }

        var room = loadedRooms[i];
        if (room == null || room.messages == null)
        {
            unreadDots[i].SetActive(false);
            continue;
        }

        int unread = 0;

        foreach (var msg in room.messages)
        {
            // Me가 보낸 메시지는 제외
            if (msg.sender == "Me") continue;

            // 자동챗이든 이미지든 텍스트든 type 상관없이 unread 처리
            if (!msg.isRead)
                unread++;
        }

        unreadDots[i].SetActive(unread > 0);
    }
} */

}
