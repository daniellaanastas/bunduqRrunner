using UnityEngine;

public static class BunduqRewardCalculator
{
    public const int SCORE_PER_POINT = 100;
    public const int COFFEE_COST = 30;
    public const int MEAL_COST = 50;
    public const int NEW_RECORD_BONUS = 1;
    public const int MAX_POINTS = MEAL_COST; 

    public static int CalculateRunRewards(int score, bool isNewRecord)
    {
        int currentStamps = PlayerPrefs.GetInt("BunduqStamps", 0);
        
        if (currentStamps >= MAX_POINTS)
            return 0;
        
        int points = score / SCORE_PER_POINT;
        
        if (isNewRecord && score >= SCORE_PER_POINT)
            points += NEW_RECORD_BONUS;
        
        int newTotal = currentStamps + points;
        if (newTotal > MAX_POINTS)
        {
            points = MAX_POINTS - currentStamps;
        }
        
        return points;
    }

    public static bool HasReachedMaxPoints()
    {
        return PlayerPrefs.GetInt("BunduqStamps", 0) >= MAX_POINTS;
    }

    public static string GetNextRewardProgress(int currentPoints)
    {
        if (currentPoints < COFFEE_COST)
            return currentPoints + " / " + COFFEE_COST + " → Free Drink";
        
        if (currentPoints < MEAL_COST)
            return currentPoints + " / " + MEAL_COST + " → Free Meal";
        
        return "Reward Ready!";
    }

    public static float GetProgressBarFill(int currentPoints)
    {
        if (currentPoints < COFFEE_COST)
        {
            return (float)currentPoints / COFFEE_COST;
        }
        else if (currentPoints < MEAL_COST)
        {
            return (float)(currentPoints - COFFEE_COST) / (MEAL_COST - COFFEE_COST);
        }
        else
        {
            return 1.0f;
        }
    }

    public static string GetCurrentGoalText(int currentPoints)
    {
        if (currentPoints < COFFEE_COST)
            return "Free Drink";
        else if (currentPoints < MEAL_COST)
            return "Free Meal";
        else
            return "Completed!";
    }


    public static string GetProgressText(int currentPoints)
    {
        if (currentPoints < COFFEE_COST)
            return currentPoints + "/" + COFFEE_COST;
        else if (currentPoints < MEAL_COST)
            return currentPoints + "/" + MEAL_COST;
        else
            return "MAX";
    }
}