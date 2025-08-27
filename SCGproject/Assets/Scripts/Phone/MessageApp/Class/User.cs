using UnityEngine;

[System.Serializable]
public class User
{
    public string id;               // 고유 식별자 (예: "me", "mom", "dad")
    public string nickname;         // 표시될 이름
    public string profileImagePath; // JSON에서 불러올 이미지 이름
    [HideInInspector] public Sprite profileImage;   
}