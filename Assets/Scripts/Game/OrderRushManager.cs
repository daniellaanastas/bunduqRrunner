using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderRushManager : MonoBehaviour
{
    public static OrderRushManager instance;
    
    [Header("Freshness")]
    public float maxFreshness = 100f;
    public float currentFreshness = 100f;
    public float normalDrainRate = 3f;
    public float obstacleDrainAmount = 20f;
    public float gemRestoreAmount = 8f;
    
    [Header("UI - Simple")]
    public Image freshnessBar;
    public TextMeshProUGUI freshnessText;
    
    private bool orderActive = true;
    
    private AudioManager cachedAudio;
    
    private float inverseMaxFreshness;
    
    private const string STATUS_FRESH = "Fresh";
    private const string STATUS_WARM = "Warm";
    private const string STATUS_GETTING_COLD = "Getting Cold!";
    private const string STATUS_COLD = "COLD!";

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        currentFreshness = maxFreshness;
        inverseMaxFreshness = 1f / maxFreshness;
        orderActive = true;
        cachedAudio = AudioManager.instance;
        UpdateUI();
    }

    void Update()
    {
        if (!PlayerManager.isGameStarted || PlayerManager.gameOver || !orderActive)
            return;

        float drain = normalDrainRate * (PlayerManager.shieldActive ? 0.3f : 1f);
        currentFreshness -= drain * Time.deltaTime;
        
        if (currentFreshness <= 0)
        {
            currentFreshness = 0;
            orderActive = false;
            PlayerManager.gameOver = true;
            
            if (cachedAudio != null)
                cachedAudio.PlaySound("GameOver");
        }
        else if (currentFreshness > maxFreshness)
        {
            currentFreshness = maxFreshness;
        }

        UpdateUI();
    }

    public void CollectFood()
    {
        currentFreshness += gemRestoreAmount;
        if (currentFreshness > maxFreshness)
            currentFreshness = maxFreshness;
    }

    public void HitObstacle()
    {
        float damage = obstacleDrainAmount * (PlayerManager.shieldActive ? 0.25f : 1f);
        currentFreshness -= damage;
        if (currentFreshness < 0)
            currentFreshness = 0;
    }

    private void UpdateUI()
    {
        if (freshnessBar == null || freshnessText == null)
            return;

        float percent = currentFreshness * inverseMaxFreshness;
        freshnessBar.fillAmount = percent;

        if (percent > 0.7f)
        {
            freshnessBar.color = Color.green;
            freshnessText.text = STATUS_FRESH;
        }
        else if (percent > 0.4f)
        {
            freshnessBar.color = Color.yellow;
            freshnessText.text = STATUS_WARM;
        }
        else if (percent > 0.15f)
        {
            freshnessBar.color = new Color(1f, 0.5f, 0f); // orange
            freshnessText.text = STATUS_GETTING_COLD;
        }
        else
        {
            freshnessBar.color = Color.red;
            freshnessText.text = STATUS_COLD;
        }
    }

    public float GetFreshnessMultiplier()
    {
        float percent = currentFreshness * inverseMaxFreshness;
        
        if (percent >= 0.9f) return 1.5f;
        if (percent >= 0.7f) return 1.3f;
        if (percent >= 0.5f) return 1.15f;
        return 1.0f;
    }
}