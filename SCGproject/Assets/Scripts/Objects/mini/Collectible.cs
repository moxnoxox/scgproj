using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum PowerEffect { Increase, Decrease }

    [Header("Configuration")]
    public PowerEffect effect = PowerEffect.Increase;
    public int powerAmount = 5;
    public float interactionDistance = 1f;
    public bool destroyOnCollect = true;

    [Header("References")]
    public key_info keyInfo; // Can be left null if not needed

    private GameObject player;
    private player_power playerPower;
    private bool isPlayerNear = false;

    void Start()
    {
        // Find player by tag for better performance and reliability
        player = GameObject.FindWithTag("Player"); 
        if (player != null)
        {
            playerPower = player.GetComponent<player_power>();
        }
        else
        {
            Debug.LogError("Player not found. Make sure the player GameObject has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (player == null || playerPower == null) return;

        float distance = Mathf.Abs(transform.position.x - player.transform.position.x);
        
        if (distance < interactionDistance)
        {
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                if (keyInfo != null) keyInfo.isObject = true;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ApplyEffect();
                if (destroyOnCollect)
                {
                    if (keyInfo != null) keyInfo.isObject = false;
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            if (isPlayerNear)
            {
                isPlayerNear = false;
                if (keyInfo != null) keyInfo.isObject = false;
            }
        }
    }

    private void ApplyEffect()
    {
        if (effect == PowerEffect.Increase)
        {
            playerPower.IncreasePower(powerAmount);
        }
        else
        {
            playerPower.DecreasePower(powerAmount);
        }
        Debug.Log($"Collected {gameObject.name}: Power {(effect == PowerEffect.Increase ? "+" : "-")}{powerAmount}");
    }
}
