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
                Debug.LogError($"❌ ChatRoomLoader: {fileName}.json 을 찾을 수 없습니다!");
                continue;
            }

            ChatRoom room = JsonUtility.FromJson<ChatRoom>(jsonFile.text);
            Debug.Log($"📂 {fileName}.json 로드 완료 → roomName: {room.roomName}");
            
            // 참가자 프로필 로드
            foreach (var user in room.participants)
            {
                string path = user.profileImagePath.Trim();
                Debug.Log($"로드 시도: '{path}'");

                if (!string.IsNullOrEmpty(path))
                {
                    // Profiles 폴더 경로 자동 보정
                    if (!path.StartsWith("Profiles/"))
                        path = "Profiles/" + path;

                    var sprite = Resources.Load<Sprite>(path);
                    if (sprite == null)
                        Debug.LogError($"❌ 참가자 {user.nickname} 프로필 로드 실패: '{path}'");
                    else
                    {
                        Debug.Log($"✅ 참가자 {user.nickname} 프로필 로드 성공: '{path}'");
                        user.profileImage = sprite;
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠ 참가자 {user.nickname} 프로필 경로 없음");
                }
            }

            loadedRooms.Add(room);
        }

        Debug.Log($"📌 최종 로드된 채팅방 수: {loadedRooms.Count}");
    }
}
