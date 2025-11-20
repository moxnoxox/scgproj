using UnityEngine;
using UnityEngine.Events;

public class BackgroundScroll : MonoBehaviour
{
    [SerializeField] private RectTransform target;

    [Header("속도 설정")]
    [SerializeField] private float peakSpeed = -800f;    // 초반 최고속도 (왼쪽 흐름이면 음수)
    [SerializeField] private float cruiseSpeed = -400f;  // 안정화 후 속도 (peak보다 느리게)

    [Header("구간 시간(초)")]
    [SerializeField] private float accelToPeakDuration = 0.8f; // 0 -> peakSpeed
    [SerializeField] private float settleDuration = 0.8f;      // peakSpeed -> cruiseSpeed
    [SerializeField] private float cruiseDuration = 4.0f;      // cruise 유지
    [SerializeField] private float decelDuration = 1.0f;       // cruiseSpeed -> 0

    private float elapsed;
    private float totalDuration;
    
    [SerializeField] private UnityEvent onScrollFinished;
    private bool finished;
    private void OnEnable() { elapsed = 0f; finished = false; }

    private void Awake()
    {
        if (target == null) target = GetComponent<RectTransform>();
        totalDuration = accelToPeakDuration + settleDuration + cruiseDuration + decelDuration;
    }

    private void Update()
    {
        if (target == null) return;
        elapsed += Time.deltaTime;
        float speed = GetSpeed(elapsed);
        target.anchoredPosition += Vector2.right * (speed * Time.deltaTime);

        if (!finished && elapsed >= totalDuration) {
        finished = true;
        onScrollFinished?.Invoke();
    }
    }

    private float GetSpeed(float t)
    {
        // 1) 0 -> peakSpeed (가속)
        if (t < accelToPeakDuration)
        {
            float u = t / accelToPeakDuration;
            return Mathf.Lerp(0f, peakSpeed, EaseOutCubic(u));
        }

        // 2) peakSpeed -> cruiseSpeed (속도 안정)
        if (t < accelToPeakDuration + settleDuration)
        {
            float u = (t - accelToPeakDuration) / settleDuration;
            return Mathf.Lerp(peakSpeed, cruiseSpeed, EaseInOutCubic(u));
        }

        // 3) cruise 유지
        if (t < accelToPeakDuration + settleDuration + cruiseDuration)
        {
            return cruiseSpeed;
        }

        // 4) cruiseSpeed -> 0 (감속)
        if (t < accelToPeakDuration + settleDuration + cruiseDuration + decelDuration)
        {
            float u = (t - accelToPeakDuration - settleDuration - cruiseDuration) / decelDuration;
            return Mathf.Lerp(cruiseSpeed, 0f, EaseInCubic(u));
        }

        return 0f; // 멈춤
    }

    private float EaseOutCubic(float u) => 1f - Mathf.Pow(1f - u, 3f);   // 초반에 훅 가속
    private float EaseInCubic(float u)  => u * u * u;                   // 서서히 감속
    private float EaseInOutCubic(float u) => u < 0.5f ? 4f*u*u*u : 1f - Mathf.Pow(-2f*u + 2f, 3f) / 2f;
}
