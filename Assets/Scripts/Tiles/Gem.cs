using UnityEngine;

public class Gem : MonoBehaviour
{
    // Cache reference instead of FindObjectOfType every collision
    private static AudioManager cachedAudio;
    private static ObjectPool cachedPool;
    private static bool cacheInitialized = false;

    private void OnEnable()
    {
        // Initialize cache once
        if (!cacheInitialized)
        {
            cachedAudio = AudioManager.instance;
            cachedPool = ObjectPool.instance;
            cacheInitialized = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Spawn effect using cached pool
        if (cachedPool != null)
        {
            GameObject effect = cachedPool.GetPooledObject();
            if (effect != null)
            {
                effect.transform.position = transform.position;
                effect.SetActive(true);
            }
        }

        // Update gems - batch this if collecting many gems
        PlayerManager.AddGems(1);
        
        // Play sound using cached reference
        if (cachedAudio != null)
            cachedAudio.PlaySound("PickUp");

        PlayerManager.score += 2;
        gameObject.SetActive(false);
    }

    // Reset cache on scene load
    public static void ResetCache()
    {
        cacheInitialized = false;
        cachedAudio = null;
        cachedPool = null;
    }
}