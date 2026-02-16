using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject[] gemContainers = new GameObject[0];
    public GameObject[] bearPickups = new GameObject[0];
    public GameObject[] obstacles = new GameObject[0];
    
    [Header("Obstacle Spawning")]
    [Range(0f, 1f)] public float baseObstacleChance = 0.3f;
    [Range(0f, 0.5f)] public float randomVariation = 0.15f;
    
    [Header("Aggressive Difficulty Scaling")]
    public float easyPhaseEnd = 50f;        // Score when easy phase ends
    public float mediumPhaseEnd = 150f;     // Score when medium phase ends
    public float hardPhaseEnd = 400f;       // Score when hard phase ends
    public float insanePhaseEnd = 800f;     // Score when insane phase ends
    
    [Header("Obstacle Chances Per Phase")]
    public float easyObstacleChance = 0.35f;
    public float mediumObstacleChance = 0.55f;
    public float hardObstacleChance = 0.75f;
    public float insaneObstacleChance = 0.88f;
    public float nightmareObstacleChance = 0.95f;
    
    [Header("Minimum Obstacles Per Tile")]
    public int easyMinObstacles = 0;
    public int mediumMinObstacles = 1;
    public int hardMinObstacles = 2;
    public int insaneMinObstacles = 3;
    public int nightmareMinObstacles = 4;
    
    [Header("Gem Spawning")]
    [Range(0f, 1f)] public float gemChance = 0.6f;
    
    [Header("Power-up")]
    public int minScoreForBears = 200;
    [Range(0f, 1f)] public float bearChance = 0.15f;
    
    [Header("Visual Variants")]
    public int mediumVisualsScore = 40;
    public int hardVisualsScore = 80;
    public GameObject[] easyGemContainers = new GameObject[0];
    public GameObject[] mediumGemContainers = new GameObject[0];
    public GameObject[] hardGemContainers = new GameObject[0];
    public GameObject[] easyObstacles = new GameObject[0];
    public GameObject[] mediumObstacles = new GameObject[0];
    public GameObject[] hardObstacles = new GameObject[0];

    public void SetupTile(int currentScore, int highScore)
    {
        SelectVisualVariants(currentScore);
        SetupObstaclesAggressive(currentScore, highScore);
        SetupGems(currentScore);
        SetupBears(currentScore);
    }

    private void SetupObstaclesAggressive(int score, int highScore)
    {
        if (obstacles == null || obstacles.Length == 0) return;
        
        // Determine difficulty phase and parameters
        float obstacleChance;
        int minObstacles;
        
        if (score < easyPhaseEnd)
        {
            // Easy: gentle introduction
            float t = score / easyPhaseEnd;
            obstacleChance = Mathf.Lerp(baseObstacleChance, easyObstacleChance, t);
            minObstacles = easyMinObstacles;
        }
        else if (score < mediumPhaseEnd)
        {
            // Medium: ramp up
            float t = (score - easyPhaseEnd) / (mediumPhaseEnd - easyPhaseEnd);
            obstacleChance = Mathf.Lerp(easyObstacleChance, mediumObstacleChance, t);
            minObstacles = mediumMinObstacles;
        }
        else if (score < hardPhaseEnd)
        {
            // Hard: serious challenge
            float t = (score - mediumPhaseEnd) / (hardPhaseEnd - mediumPhaseEnd);
            obstacleChance = Mathf.Lerp(mediumObstacleChance, hardObstacleChance, t);
            minObstacles = hardMinObstacles;
        }
        else if (score < insanePhaseEnd)
        {
            // Insane: near-maximum density
            float t = (score - hardPhaseEnd) / (insanePhaseEnd - hardPhaseEnd);
            obstacleChance = Mathf.Lerp(hardObstacleChance, insaneObstacleChance, t);
            minObstacles = insaneMinObstacles;
        }
        else
        {
            // Nightmare: endless scaling beyond insane
            float beyondInsane = score - insanePhaseEnd;
            float extraScale = Mathf.Min(beyondInsane / 500f, 0.07f); // Caps at +7% more
            obstacleChance = Mathf.Min(nightmareObstacleChance + extraScale, 0.98f);
            minObstacles = nightmareMinObstacles;
            
            // Every 200 score beyond insane, try to add another minimum obstacle
            int extraMin = Mathf.FloorToInt(beyondInsane / 200f);
            minObstacles = Mathf.Min(minObstacles + extraMin, obstacles.Length - 1);
        }
        
        // Bonus difficulty if player is beating their high score
        if (highScore > 0 && score > highScore)
        {
            float beyondRecord = score - highScore;
            float recordBonus = Mathf.Min(beyondRecord / 100f * 0.05f, 0.15f);
            obstacleChance = Mathf.Min(obstacleChance + recordBonus, 0.98f);
        }
        
        // Count how many we activate
        int activatedCount = 0;
        
        // First pass: random activation based on chance
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle == null) continue;
            
            float finalChance = obstacleChance + Random.Range(-randomVariation, randomVariation);
            bool shouldActivate = Random.value < finalChance;
            
            obstacle.SetActive(shouldActivate);
            if (shouldActivate) activatedCount++;
        }
        
        // Second pass: ensure minimum obstacles are met
        if (activatedCount < minObstacles)
        {
            int needed = minObstacles - activatedCount;
            
            // Shuffle indices to pick random inactive obstacles
            int[] indices = new int[obstacles.Length];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;
            
            // Fisher-Yates shuffle
            for (int i = indices.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
            }
            
            // Activate random inactive obstacles until minimum is met
            foreach (int idx in indices)
            {
                if (needed <= 0) break;
                if (obstacles[idx] != null && !obstacles[idx].activeSelf)
                {
                    obstacles[idx].SetActive(true);
                    needed--;
                }
            }
        }
    }

    private void SetupGems(int score)
    {
        if (gemContainers == null || gemContainers.Length == 0) return;
        
        float adjustedGemChance = gemChance;
        if (score > hardPhaseEnd)
        {
            float reduction = Mathf.Min((score - hardPhaseEnd) / 1000f, 0.25f);
            adjustedGemChance = Mathf.Max(gemChance - reduction, 0.35f);
        }
        
        foreach (GameObject container in gemContainers)
        {
            if (container == null) continue;
            
            if (Random.value < adjustedGemChance)
            {
                container.SetActive(true);
                
                foreach (Transform gem in container.transform)
                {
                    gem.gameObject.SetActive(true);
                }
            }
            else
            {
                container.SetActive(false);
            }
        }
    }
    private void SetupBears(int score)
    {
        if (bearPickups == null || bearPickups.Length == 0) return;
        
        // Bears become rarer at extreme difficulty (but never zero)
        float adjustedBearChance = bearChance;
        if (score > insanePhaseEnd)
        {
            float reduction = Mathf.Min((score - insanePhaseEnd) / 1000f, 0.08f);
            adjustedBearChance = Mathf.Max(bearChance - reduction, 0.07f);
        }
        
        foreach (GameObject bear in bearPickups)
        {
            if (bear == null) continue;
            bear.SetActive(score >= minScoreForBears && Random.value < adjustedBearChance);
        }
    }

    private void SelectVisualVariants(int currentScore)
    {
        DeactivateAllVariants();
        if (currentScore < mediumVisualsScore)
        {
            if (easyGemContainers.Length > 0) gemContainers = easyGemContainers;
            if (easyObstacles.Length > 0) obstacles = easyObstacles;
        }
        else if (currentScore < hardVisualsScore)
        {
            if (mediumGemContainers.Length > 0) gemContainers = mediumGemContainers;
            if (mediumObstacles.Length > 0) obstacles = mediumObstacles;
        }
        else
        {
            if (hardGemContainers.Length > 0) gemContainers = hardGemContainers;
            if (hardObstacles.Length > 0) obstacles = hardObstacles;
        }
    }

    private void DeactivateAllVariants()
    {
        DeactivateArray(easyGemContainers);
        DeactivateArray(mediumGemContainers);
        DeactivateArray(hardGemContainers);
        DeactivateArray(easyObstacles);
        DeactivateArray(mediumObstacles);
        DeactivateArray(hardObstacles);
    }

    private void DeactivateArray(GameObject[] array)
    {
        if (array == null) return;
        foreach (GameObject obj in array)
            if (obj != null) obj.SetActive(false);
    }
}