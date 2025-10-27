using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class ChoiceData
{
    public string text;
    public string resultMessage; // 실제 채팅에 추가될 메시지 (optional)
    public string action; // optional
    public string timestamp;  
    public bool isRead;

    public List<ReplyData> replies;

    public List<string> monologueText;
    public int energyCost;
}

[System.Serializable]
public class ReplyData
{
    public string sender;
    public string content;
    public float delayAfter; 
    public string timestamp;
    public bool isRead;
}

[System.Serializable]
public class Message
{
    public string type = "message"; 
    public string sender;
    public string content;
    public string timestamp;
    public bool isRead;
    public float delayAfter; 

    public bool isConsumed = false; 
    public List<ChoiceData> choices; 

    // 🔹 추가된 필드: 이미지 / 텍스트 구분용
    public string format = "text";  

    public Message(string sender, string content, string gameTime)
    {
        this.type = "message";
        this.sender = sender;
        this.content = content;
        this.timestamp = gameTime; 
        this.isRead = false;
        this.format = "text"; // 기본값
    }

    public Message(string type)
    {
        this.type = type;
        this.format = "text";
    }
}


