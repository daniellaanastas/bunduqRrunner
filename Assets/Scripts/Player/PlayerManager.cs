using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    public static bool gameOver;
    public GameObject gameOverPanel;
    public static bool isGameStarted;
    public GameObject startingText;
    public GameObject newRecordPanel;
    public static int score;
    public Text scoreText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI newRecordText;
    public static bool isGamePaused;
    public GameObject[] characterPrefabs;
    public static bool shieldActive;
    public static float shieldTime;
    public static float speedBeforeShield;
    public GameObject rewardUI;
    public TextMeshProUGUI rewardText;
    public Image progressBarFill;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI goalText;

    // CACHED REFERENCES - avoid FindObjectOfType every frame
    private ScreenFlash cachedScreenFlash;
    private PlayerController cachedPlayer;
    private AudioManager cachedAudio;
    
    // CACHED VALUES - avoid PlayerPrefs reads every frame
    private int cachedTotalGems;
    private int cachedHighScore;
    private int cachedStamps;
    private bool gameOverProcessed = false;
    
    // Reusable string builder to avoid allocations
    private System.Text.StringBuilder sb = new System.Text.StringBuilder(32);

    private void Awake()
    {
        int index = PlayerPrefs.GetInt("SelectedCharacter");
        GameObject go = Instantiate(characterPrefabs[index], transform.position, Quaternion.identity);
        
        cachedPlayer = go.GetComponent<PlayerController>();
        if (cachedPlayer != null)
        {
            if (cachedPlayer.groundCheck == null)
            {
                cachedPlayer.groundCheck = go.transform.Find("groundCheck");
            }
            if (cachedPlayer.groundLayer == 0)
            {
                cachedPlayer.groundLayer = LayerMask.GetMask("Ground");
            }
        }
    }

    void Start()
    {
        score = 0;
        Time.timeScale = 1;
        gameOver = isGameStarted = isGamePaused = false;
        shieldActive = false;
        shieldTime = 0;
        gameOverProcessed = false;
        
        // Cache references ONCE
        cachedScreenFlash = FindObjectOfType<ScreenFlash>();
        cachedAudio = AudioManager.instance;
        
        // Cache PlayerPrefs values ONCE at start
        cachedTotalGems = PlayerPrefs.GetInt("TotalGems", 0);
        cachedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        cachedStamps = PlayerPrefs.GetInt("BunduqStamps", 0);

        if (newRecordPanel != null)
            newRecordPanel.SetActive(false);
        if (rewardUI != null)
            rewardUI.SetActive(false);

        UpdateProgressBar(cachedStamps);
        CheckForNewlyUnlockedRewards(cachedStamps);
    }

    private void CheckForNewlyUnlockedRewards(int currentStamps)
    {
        int previousStamps = PlayerPrefs.GetInt("PreviousStamps", 0);
        
        bool justUnlockedDrink = previousStamps < BunduqRewardCalculator.COFFEE_COST && 
                                  currentStamps >= BunduqRewardCalculator.COFFEE_COST &&
                                  !PlayerPrefs.HasKey("DrinkClaimed");
        
        bool justUnlockedMeal = previousStamps < BunduqRewardCalculator.MEAL_COST && 
                                 currentStamps >= BunduqRewardCalculator.MEAL_COST &&
                                 !PlayerPrefs.HasKey("MealClaimed");

        if (justUnlockedMeal)
            ShowRewardMessage("You earned a Free Meal!");
        else if (justUnlockedDrink)
            ShowRewardMessage("You earned a Free Drink!");

        PlayerPrefs.SetInt("PreviousStamps", currentStamps);
        PlayerPrefs.Save();
    }

    private void ShowRewardMessage(string message)
    {
        if (rewardUI != null && rewardText != null)
        {
            rewardUI.SetActive(true);
            rewardText.text = message;
            StartCoroutine(HideRewardUIAfterDelay(3f));
        }
    }

    private IEnumerator HideRewardUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rewardUI != null)
            rewardUI.SetActive(false);
    }

    void Update()
    {
        // Update UI without allocations - use cached StringBuilder
        sb.Clear();
        sb.Append(cachedTotalGems);
        gemsText.text = sb.ToString();
        
        sb.Clear();
        sb.Append("Score: ");
        sb.Append(score);
        scoreText.text = sb.ToString();

        // Shield logic with cached reference
        if (shieldActive)
        {
            shieldTime -= Time.deltaTime;
            if (shieldTime <= 0)
            {
                shieldActive = false;
                if (cachedScreenFlash != null)
                    cachedScreenFlash.DeactivateShield();
                    
                if (cachedPlayer != null)
                {
                    float restoredSpeed = Mathf.Min(speedBeforeShield, cachedPlayer.maxSpeed);
                    cachedPlayer.forwardSpeed = Mathf.Clamp(restoredSpeed, 0.1f, cachedPlayer.maxSpeed);
                }
            }
        }

        // Game Over - process only ONCE
        if (gameOver && !gameOverProcessed)
        {
            ProcessGameOver();
        }

        // Start Game
        if (SwipeManager.tap && !isGameStarted)
        {
            isGameStarted = true;
            if (startingText != null)
                Destroy(startingText);
            if (rewardUI != null)
                rewardUI.SetActive(false);
        }
    }

    private void ProcessGameOver()
    {
        gameOverProcessed = true;
        Time.timeScale = 0;
        
        bool isNewRecord = score > cachedHighScore;
        int earnedPoints = BunduqRewardCalculator.CalculateRunRewards(score, isNewRecord);

        if (isNewRecord)
        {
            if (newRecordPanel != null)
            {
                newRecordPanel.SetActive(true);
                sb.Clear();
                sb.Append("Your Fastest \nDelivery!\n");
                sb.Append(score);
                newRecordText.text = sb.ToString();
            }
            PlayerPrefs.SetInt("HighScore", score);
            cachedHighScore = score;
            
            if (rewardUI != null)
                rewardUI.SetActive(false);
        }
        else
        {
            if (newRecordPanel != null)
                newRecordPanel.SetActive(false);
                
            if (rewardUI != null && earnedPoints > 0)
            {
                rewardUI.SetActive(true);
                sb.Clear();
                sb.Append("Successful Delivery!\nYou earned ");
                sb.Append(earnedPoints);
                sb.Append(" Bunduq Point");
                if (earnedPoints > 1) sb.Append("s");
                sb.Append("!");
                if (isNewRecord) sb.Append("\nGreat Run! +1 Point Bonus");
                rewardText.text = sb.ToString();
            }
        }

        PlayerPrefs.SetInt("LastEarnedStamps", earnedPoints);
        cachedStamps += earnedPoints;
        PlayerPrefs.SetInt("BunduqStamps", cachedStamps);
        PlayerPrefs.Save();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
            
        Destroy(gameObject);
    }

    // Call this when gems are collected to update cached value
    public static void AddGems(int amount)
    {
        PlayerManager pm = FindObjectOfType<PlayerManager>();
        if (pm != null)
        {
            pm.cachedTotalGems += amount;
            PlayerPrefs.SetInt("TotalGems", pm.cachedTotalGems);
        }
    }

    private void UpdateProgressBar(int currentPoints)
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = BunduqRewardCalculator.GetProgressBarFill(currentPoints);
            
            if (currentPoints >= BunduqRewardCalculator.MEAL_COST)
                progressBarFill.color = new Color(1f, 0.84f, 0f);
            else if (currentPoints >= BunduqRewardCalculator.COFFEE_COST)
                progressBarFill.color = new Color(0.2f, 0.6f, 1f);
            else if (currentPoints >= 7)
                progressBarFill.color = new Color(1f, 0.92f, 0.016f);
            else
                progressBarFill.color = new Color(1f, 0.5f, 0f);
        }

        if (progressText != null)
            progressText.text = BunduqRewardCalculator.GetProgressText(currentPoints);

        if (goalText != null)
            goalText.text = BunduqRewardCalculator.GetCurrentGoalText(currentPoints);
    }
}