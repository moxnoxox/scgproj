using UnityEngine;
using System.Collections;

public class SoundManagerCh2 : MonoBehaviour
{
    public static SoundManagerCh2 Instance;
    public AudioSource plasticbag;
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void PlayPlasticBag()
    {
        StartCoroutine(PlayPlasticBagCoroutine());
    }
    IEnumerator PlayPlasticBagCoroutine()
    {
        //0.5초부터 1초간 플레이
        plasticbag.Play();
        yield return new WaitForSeconds(1f);
        plasticbag.Stop();
    }
}
