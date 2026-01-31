using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryManager : MonoBehaviour
{
    public GameObject victoryPanel;
    public Button retryButton;
    public Button startOverButton; 

    private bool isGamePaused = false; // Track if the game is paused

    private void Start()
    {
        victoryPanel.SetActive(false); // Ensure the panel is hidden at start
        retryButton.onClick.AddListener(RetryLevel);
        startOverButton.onClick.AddListener(StartOver);
    }

    public void Victory()
    {
        victoryPanel.SetActive(true); // Show the victory panel
        PauseGame();                   // Pause the game
    }

    private void RetryLevel()
    {
        Debug.Log("Retry Level button clicked");
        ResumeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void StartOver()
    {
        Debug.Log("Start Over button clicked");
        ResumeGame();
        SceneManager.LoadScene(0); // Adjust the index as necessary for your main scene
    }


    private void PauseGame()
    {
        Time.timeScale = 0f; // Stop the game
        isGamePaused = true;
        UnlockCursor();       // Unlock the mouse cursor
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game
        isGamePaused = false;
        LockCursor();        // Lock the mouse cursor if needed
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true;                    // Make the cursor visible
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false;                    // Make the cursor invisible
    }
}