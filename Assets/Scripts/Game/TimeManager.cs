using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    public void SlowMotion(float slowFactor, float duration)
    {
        StartCoroutine(SlowMotionEffect(slowFactor, duration));
    }
    
    private IEnumerator SlowMotionEffect(float slowFactor, float duration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = slowFactor;
        // REMOVED: Time.fixedDeltaTime = 0.02f * Time.timeScale;
        
        yield return new WaitForSecondsRealtime(duration);
        
        if (!PlayerManager.gameOver)
        {
            Time.timeScale = originalTimeScale;
            // REMOVED: Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }
}