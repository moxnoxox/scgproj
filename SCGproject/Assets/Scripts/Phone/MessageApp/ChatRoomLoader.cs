using System.Collections.Generic;
using UnityEngine;

public class ChatRoomLoader : MonoBehaviour
{
    [Header("JSON 데이터 파일들 (Resources/ChatData 폴더 안)")]
    public string[] chatJsonFiles; // 예: { "mom", "friend1" }

    [HideInInspector]
    public List<ChatRoom> loadedRooms = new List<ChatRoom>();

    private void Awake()
    {
        LoadAllChatRooms();
    }

    public void LoadAllChatRooms()
    {
        loadedRooms.Clear();

        foreach (string fileName in chatJsonFiles)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>($"ChatData/{fileName}");
            if (jsonFile == null)
            {
                Debug.LogError($"ChatRoomLoader: {fileName}.json 을 찾을 수 없습니다!");
                continue;
            }

            ChatRoom room = JsonUtility.FromJson<ChatRoom>(jsonFile.text);

            // Sprite 로드 (profileImagePath 기반)
            if (!string.IsNullOrEmpty(room.profileImagePath))
                room.profileImage = Resources.Load<Sprite>(room.profileImagePath);

            // 프로필 로드
            foreach (var user in room.participants)
            {
                if (!string.IsNullOrEmpty(user.profileImagePath))
                    user.profileImage = Resources.Load<Sprite>(user.profileImagePath);
            }

            loadedRooms.Add(room);
        }

        
        Debug.Log($"로드된 채팅방 수: {loadedRooms.Count}");
    }
}
