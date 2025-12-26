using UnityEngine;

public class SoundManagerCh3 : MonoBehaviour
{
    public static SoundManagerCh3 Instance;

    [Header("Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip ch3Bgm;
    public AudioClip cookieBgm;
    public AudioClip busRide;
    public AudioClip busLeave;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    public void PlayCh3Bgm(bool loop = true)  { PlayBgmInternal(ch3Bgm, loop); }
    public void PlayCookieBgm(bool loop = true)  { PlayBgmInternal(cookieBgm, loop); }
    public void PlayBusRide()  { PlaySfx(busRide); }
    public void PlayBusLeave() { PlaySfx(busLeave); }

    void PlayBgmInternal(AudioClip clip, bool loop)
    {
        if (bgmSource == null || clip == null) return;
        bgmSource.loop = loop;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBgm() { if (bgmSource) bgmSource.Stop(); }

    void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
