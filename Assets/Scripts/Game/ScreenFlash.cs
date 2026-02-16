using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFlash : MonoBehaviour
{
    public ParticleSystem shieldParticles;
    public float shieldDuration = 6f;
    
    [Header("Game Over Effects")]
    public Image flashImage;
    public Color flashColor = new Color(1f, 0.2f, 0f, 0.6f);
    public float flashDuration = 0.5f;

    // Pre-allocated color to avoid GC
    private Color workingColor;
    private Coroutine flashCoroutine;

    void Start()
    {
        if (flashImage != null)
        {
            workingColor = flashImage.color;
            workingColor.a = 0;
            flashImage.color = workingColor;
        }
    }

    public void FlashWithShield()
    {
        ActivateShield();
    }

    void ActivateShield()
    {
        PlayerManager.shieldActive = true;
        PlayerManager.shieldTime = shieldDuration;
        
        if (shieldParticles != null)
            shieldParticles.Play();
    }

    public void DeactivateShield()
    {
        PlayerManager.shieldActive = false;
        
        if (shieldParticles != null)
            shieldParticles.Stop();
    }

    public void FlashGameOver()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashSequence());
    }

    private IEnumerator FlashSequence()
    {
        if (flashImage == null) yield break;

        float elapsed = 0;
        float halfDuration = flashDuration * 0.5f;

        // Flash in
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            workingColor.r = flashColor.r;
            workingColor.g = flashColor.g;
            workingColor.b = flashColor.b;
            workingColor.a = Mathf.Lerp(0, flashColor.a, elapsed / halfDuration);
            flashImage.color = workingColor;
            yield return null;
        }

        // Flash out
        elapsed = 0;
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            workingColor.a = Mathf.Lerp(flashColor.a, 0, elapsed / flashDuration);
            flashImage.color = workingColor;
            yield return null;
        }

        workingColor.a = 0;
        flashImage.color = workingColor;
        flashCoroutine = null;
    }
}