using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI gemsText;
    public Animator messageAnim;
    public TextMeshProUGUI freeMealsText;
    public GameObject congratsPanel;
    public TextMeshProUGUI congratsText;

    private int cachedHighScore;
    private int cachedGems;
    private string highScoreString;
    private string gemsString;

    private void Start()
    {
        Time.timeScale = 1;
        congratsPanel.SetActive(false);

        cachedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        cachedGems = PlayerPrefs.GetInt("TotalGems", 0);
        int lastEarned = PlayerPrefs.GetInt("LastEarnedStamps", 0);
        int stamps = PlayerPrefs.GetInt("BunduqStamps", 0);

        highScoreString = "High Score\n" + cachedHighScore;
        gemsString = cachedGems.ToString();

        highScoreText.text = highScoreString;
        gemsText.text = gemsString;

        if (lastEarned > 0)
        {
            congratsPanel.SetActive(true);
            string pointWord = lastEarned == 1 ? "Point" : "Points";
            congratsText.text = "Delivery Complete!\nYou earned " + lastEarned + " Bunduq " + pointWord + "!";
            freeMealsText.text = "Your Best Run: " + cachedHighScore + "\n\nYour Bunduq Points: " + stamps;

            PlayerPrefs.SetInt("LastEarnedStamps", 0);
            PlayerPrefs.Save();
        }
        else
        {
            congratsPanel.SetActive(false);
            if (freeMealsText != null)
            {
                freeMealsText.text = "Your Best Run: " + cachedHighScore + "\n\nYour Bunduq Points: ";
            }
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}