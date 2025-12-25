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

    private Dictionary<string, Sprite> roomSpriteMap = new Dictionary<string, Sprite>();


    private void Start()
    {
        // ✔ 챕터1, 챕터2, 챕터3 어디서든 동일하게 데이터 가져오기
        loadedRooms = PhoneDataManager.Instance.chatRooms;

        if (loadedRooms == null || loadedRooms.Count == 0)
        {
            Debug.LogError("ChatRoomButtonManager: PhoneDataManager에 chatRooms가 없습니다.");
            return;
        }

        CacheRoomSprites();   
        RefreshRoomList();

        /*

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
        */

        // 초기 갱신
        UpdateUnreadDots();
    }

    private void OnEnable()
    {
        CacheRoomSprites();  
        RefreshRoomList();
    }

    private void CacheRoomSprites()
    {
        roomSpriteMap = PhoneDataManager.Instance.roomSpriteMap;
        if (roomSpriteMap == null) {
            roomSpriteMap = new Dictionary<string, Sprite>();
            PhoneDataManager.Instance.roomSpriteMap = roomSpriteMap;
        }

        var rooms = PhoneDataManager.Instance.chatRooms;
        for (int i = 0; i < roomButtons.Count && i < rooms.Count; i++)
        {
            var img = roomButtons[i].GetComponent<Image>();
            if (img != null && img.sprite != null && !roomSpriteMap.ContainsKey(rooms[i].roomName))
                roomSpriteMap[rooms[i].roomName] = img.sprite; // 처음 씬에서만 채워짐
        }
    }


    public void UpdateUnreadDots()
    {
        loadedRooms = PhoneDataManager.Instance.chatRooms;
        if (loadedRooms == null || unreadDots == null) return;

        for (int i = 0; i < loadedRooms.Count && i < unreadDots.Count; i++)
        {
            ChatRoom room = loadedRooms[i];

            bool notLoadedYet = room.messages == null || room.messages.Count == 0;
            bool hasUnread = room.UnreadCount > 0;

            unreadDots[i].SetActive(notLoadedYet || hasUnread);
        }
    }

    public void RefreshRoomList()
    {
        loadedRooms = PhoneDataManager.Instance.chatRooms;

        for (int i = 0; i < roomButtons.Count; i++)
        {
            var btn = roomButtons[i];
            btn.onClick.RemoveAllListeners();

            // 방 개수보다 버튼이 많다면 버튼/미읽음 숨기기
            if (i >= loadedRooms.Count)
            {
                btn.gameObject.SetActive(false);
                if (i < unreadDots.Count) unreadDots[i].SetActive(false);
                continue;
            }

            btn.gameObject.SetActive(true);
            ChatRoom room = loadedRooms[i];

            // 버튼 이미지 교체 
            var img = btn.GetComponent<Image>();
            if (img != null && roomSpriteMap != null &&
                roomSpriteMap.TryGetValue(room.roomName, out var sprite) && sprite != null)
            {
                img.sprite = sprite;
            }

            // 클릭 시 해당 방 열고 읽음 처리
            btn.onClick.AddListener(() =>
            {
                chatAppManager.OpenChatRoomWithData(room);
                foreach (var msg in room.messages)
                    if (msg.sender != "Me") msg.isRead = true;
                UpdateUnreadDots();
            });
        }

        UpdateUnreadDots();

        // 필요하면 dummyButtons에 대한 onClick도 여기서 설정
        foreach (var dummy in dummyButtons)
        {
            dummy.onClick.RemoveAllListeners();
            dummy.onClick.AddListener(() =>
            {
                MonologueManager.Instance.ShowMonologuesSequentially(
                    new List<string> { "여긴 별 볼일 없어..." },
                    3f
                );
            });
        }
    }


}
