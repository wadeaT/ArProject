using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Call this from your "Finish" button OnClick event
    public void LoadQuizScene()
    {
        // Make sure the exact name of your second scene is "QuizScene"
        SceneManager.LoadScene("QuizScene");
    }

    // Optional: Call this to go back to AR mode
    public void LoadARScene()
    {
        SceneManager.LoadScene(0); // Assuming AR scene is at index 0
    }
}