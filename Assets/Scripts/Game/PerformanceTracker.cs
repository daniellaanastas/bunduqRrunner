using UnityEngine;

public class PerformanceTracker : MonoBehaviour
{
    public static PerformanceTracker instance;
    
    [Header("Performance Metrics")]
    public float performanceScore = 0.5f; // 0 = struggling, 1 = dominating
    public float recentPerformance = 0.5f; // Short-term performance
    
    [Header("Tracking")]
    public int gemsCollectedRecently = 0;
    public int obstaclesHitRecently = 0;
    public int closeCallsRecently = 0; // Near misses
    public float timeSinceLastHit = 0f;
    public float longestSurvivalStreak = 0f;
    
    [Header("Tuning")]
    public float performanceDecayRate = 0.02f;
    public float gemBoostAmount = 0.03f;
    public float obstacleHitPenalty = 0.15f;
    public float survivalBonus = 0.005f;
    public float closeCallBonus = 0.02f;
    
    // Rolling window for recent performance
    private float[] recentScores = new float[10];
    private int scoreIndex = 0;
    private float windowUpdateTimer = 0f;
    
    // Streak tracking
    private int currentGemStreak = 0;
    private int currentSurvivalTiles = 0;
    
    // Difficulty output
    public float DifficultyMultiplier => Mathf.Lerp(0.6f, 1.4f, performanceScore);
    public float GemChanceBonus => Mathf.Lerp(0.2f, -0.1f, performanceScore); // More gems when struggling
    public float ObstacleChanceModifier => Mathf.Lerp(0.7f, 1.3f, performanceScore); // Fewer obstacles when struggling
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        ResetPerformance();
    }
    
    public void ResetPerformance()
    {
        performanceScore = 0.5f;
        recentPerformance = 0.5f;
        gemsCollectedRecently = 0;
        obstaclesHitRecently = 0;
        closeCallsRecently = 0;
        timeSinceLastHit = 0f;
        longestSurvivalStreak = 0f;
        currentGemStreak = 0;
        currentSurvivalTiles = 0;
        
        for (int i = 0; i < recentScores.Length; i++)
            recentScores[i] = 0.5f;
        scoreIndex = 0;
    }
    
    void Update()
    {
        if (!PlayerManager.isGameStarted || PlayerManager.gameOver)
            return;
        
        // Track survival time
        timeSinceLastHit += Time.deltaTime;
        
        // Survival bonus - reward staying alive
        if (timeSinceLastHit > 5f)
        {
            performanceScore = Mathf.Min(1f, performanceScore + survivalBonus * Time.deltaTime);
        }
        
        // Update rolling window
        windowUpdateTimer += Time.deltaTime;
        if (windowUpdateTimer >= 3f) // Every 3 seconds
        {
            UpdateRollingWindow();
            windowUpdateTimer = 0f;
        }
        
        // Natural decay toward middle
        float target = 0.5f;
        performanceScore = Mathf.MoveTowards(performanceScore, target, performanceDecayRate * Time.deltaTime);
    }
    
    private void UpdateRollingWindow()
    {
        recentScores[scoreIndex] = performanceScore;
        scoreIndex = (scoreIndex + 1) % recentScores.Length;
        
        // Calculate recent average
        float sum = 0f;
        for (int i = 0; i < recentScores.Length; i++)
            sum += recentScores[i];
        recentPerformance = sum / recentScores.Length;
    }
    
    // Call when player collects a gem
    public void OnGemCollected()
    {
        gemsCollectedRecently++;
        currentGemStreak++;
        
        // Streak bonus
        float streakBonus = Mathf.Min(currentGemStreak * 0.005f, 0.03f);
        performanceScore = Mathf.Min(1f, performanceScore + gemBoostAmount + streakBonus);
    }
    
    // Call when player hits an obstacle
    public void OnObstacleHit()
    {
        obstaclesHitRecently++;
        timeSinceLastHit = 0f;
        currentGemStreak = 0;
        currentSurvivalTiles = 0;
        
        performanceScore = Mathf.Max(0f, performanceScore - obstacleHitPenalty);
    }
    
    // Call when player narrowly avoids an obstacle
    public void OnCloseCall()
    {
        closeCallsRecently++;
        performanceScore = Mathf.Min(1f, performanceScore + closeCallBonus);
    }
    
    // Call when player completes a tile
    public void OnTileCompleted()
    {
        currentSurvivalTiles++;
        
        if (currentSurvivalTiles > longestSurvivalStreak)
            longestSurvivalStreak = currentSurvivalTiles;
        
        // Small bonus for each tile survived
        performanceScore = Mathf.Min(1f, performanceScore + 0.008f);
    }
    
    // Call when player collects a bear
    public void OnPowerUpCollected()
    {
        // Power-ups indicate good positioning
        performanceScore = Mathf.Min(1f, performanceScore + 0.05f);
    }
    
    // Get difficulty level for tile generation
    public DifficultyLevel GetCurrentDifficulty()
    {
        // Combine performance with score progression
        float scoreFactor = Mathf.Clamp01(PlayerManager.score / 500f);
        float combinedDifficulty = (performanceScore * 0.6f) + (scoreFactor * 0.4f);
        
        if (combinedDifficulty < 0.25f) return DifficultyLevel.Easy;
        if (combinedDifficulty < 0.45f) return DifficultyLevel.Medium;
        if (combinedDifficulty < 0.65f) return DifficultyLevel.Hard;
        if (combinedDifficulty < 0.85f) return DifficultyLevel.Insane;
        return DifficultyLevel.Nightmare;
    }
    
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Insane,
        Nightmare
    }
    
    // Get spawn parameters based on performance
    public SpawnParameters GetSpawnParameters()
    {
        var difficulty = GetCurrentDifficulty();
        var param = new SpawnParameters();
        
        // Base values by difficulty
        switch (difficulty)
        {
            case DifficultyLevel.Easy:
                param.obstacleChance = 0.25f;
                param.minObstacles = 0;
                param.gemChance = 0.85f;
                param.bearChance = 0.20f;
                param.eventChance = 0.25f;
                break;
                
            case DifficultyLevel.Medium:
                param.obstacleChance = 0.45f;
                param.minObstacles = 1;
                param.gemChance = 0.75f;
                param.bearChance = 0.18f;
                param.eventChance = 0.30f;
                break;
                
            case DifficultyLevel.Hard:
                param.obstacleChance = 0.60f;
                param.minObstacles = 2;
                param.gemChance = 0.65f;
                param.bearChance = 0.15f;
                param.eventChance = 0.35f;
                break;
                
            case DifficultyLevel.Insane:
                param.obstacleChance = 0.75f;
                param.minObstacles = 3;
                param.gemChance = 0.55f;
                param.bearChance = 0.12f;
                param.eventChance = 0.40f;
                break;
                
            case DifficultyLevel.Nightmare:
                param.obstacleChance = 0.88f;
                param.minObstacles = 4;
                param.gemChance = 0.45f;
                param.bearChance = 0.10f;
                param.eventChance = 0.45f;
                break;
        }
        
        // Apply performance modifiers
        // Struggling? More gems, fewer obstacles
        if (performanceScore < 0.3f)
        {
            param.gemChance += 0.15f;
            param.obstacleChance -= 0.15f;
            param.bearChance += 0.08f;
        }
        // Dominating? Slightly fewer gems, more obstacles
        else if (performanceScore > 0.7f)
        {
            param.gemChance -= 0.05f;
            param.obstacleChance += 0.10f;
        }
        
        // Freshness-based adjustment
        if (OrderRushManager.instance != null)
        {
            float freshness = OrderRushManager.instance.currentFreshness / OrderRushManager.instance.maxFreshness;
            if (freshness < 0.3f)
            {
                // Desperate - give more gems and bears
                param.gemChance += 0.20f;
                param.bearChance += 0.15f;
                param.obstacleChance -= 0.10f;
            }
        }
        
        // Clamp values
        param.obstacleChance = Mathf.Clamp(param.obstacleChance, 0.1f, 0.92f);
        param.gemChance = Mathf.Clamp(param.gemChance, 0.35f, 0.95f);
        param.bearChance = Mathf.Clamp(param.bearChance, 0.08f, 0.35f);
        
        return param;
    }
    
    public struct SpawnParameters
    {
        public float obstacleChance;
        public int minObstacles;
        public float gemChance;
        public float bearChance;
        public float eventChance;
    }
}