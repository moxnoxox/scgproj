using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 임시 시간임. 추후 수정 가능!!
public class GameClock : MonoBehaviour
{
    public static GameClock Instance { get; private set; }

    public int hour = 9;
    public int minute = 0;

    public float timeScale = 60f;  // 1초 = 1분 (배속 조절)
    private float timer;

    private void Awake()
    {
        // 싱글톤 유지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환해도 유지
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    private void Update()
    {
        timer += Time.deltaTime * timeScale;
        if (timer >= 60f)
        {
            minute++;
            timer = 0f;

            if (minute >= 60)
            {
                minute = 0;
                hour++;
                if (hour >= 24) hour = 0;
            }
        }
    }

    public string GetTimeString()
    {
        return string.Format("{0:D2}:{1:D2}", hour, minute);
    }
}
