using UnityEngine;

public class BearPickup : MonoBehaviour
{
    public float shieldDuration = 6f;
    public float speedBoost = 7f;
    
    // Static cached references - shared across all instances
    private static AudioManager cachedAudio;
    private static ScreenFlash cachedScreenFlash;
    private static bool cacheInitialized = false;

    private void OnEnable()
    {
        // Initialize cache once
        if (!cacheInitialized)
        {
            cachedAudio = AudioManager.instance;
            cachedScreenFlash = FindObjectOfType<ScreenFlash>();
            cacheInitialized = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            if (!PlayerManager.shieldActive)
                PlayerManager.speedBeforeShield = player.forwardSpeed;

            player.forwardSpeed = Mathf.Min(player.forwardSpeed + speedBoost, player.maxSpeed);
        }

        PlayerManager.shieldActive = true;
        PlayerManager.shieldTime = shieldDuration;

        if (cachedAudio != null)
            cachedAudio.PlaySound("PowerUp");

        if (cachedScreenFlash != null)
            cachedScreenFlash.FlashWithShield();

        gameObject.SetActive(false);
    }

    // Reset cache on scene load
    public static void ResetCache()
    {
        cacheInitialized = false;
        cachedAudio = null;
        cachedScreenFlash = null;
    }
}