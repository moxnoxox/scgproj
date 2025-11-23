using UnityEngine;

public class bgm_controller : MonoBehaviour
{
    public static bgm_controller Instance;
    public Camera mainCamera;
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        mainCamera = Camera.main;
    }
    void Update()
    {
        this.gameObject.transform.position = mainCamera.transform.position;
        if(mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
}
