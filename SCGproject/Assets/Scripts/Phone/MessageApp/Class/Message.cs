using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Message
{
    public string type = "message";
    public string sender;
    public string content;
    public string timestamp;
    public bool isRead;

    public Message(string sender, string content, string gameTime)
    {
        this.type = "message";
        this.sender = sender;
        this.content = content;
        this.timestamp = gameTime; // GameClock에서 받아온 시간
        this.isRead = false;
    }

    public Message(string type)
    {
        this.type = type;
    }
}
