using UnityEngine;
using UnityEngine.UI; // Required for Button
using TMPro; // Required for TextMeshPro
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI feedbackText; // To show "Correct!" or "Wrong"
    public GameObject restartButton; // Button to restart or go back

    private List<Question> questions;
    private int currentQuestionIndex = 0;
    private int score = 0;
    private bool isQuizFinished = false;

    void Start()
    {
        questions = QuizData.GetQuestions();
        restartButton.SetActive(false);
        feedbackText.text = "";
        ShowQuestion();
    }

    void ShowQuestion()
    {
        if (currentQuestionIndex >= questions.Count)
        {
            FinishQuiz();
            return;
        }

        Question q = questions[currentQuestionIndex];
        questionText.text = q.questionText;
        feedbackText.text = ""; // Clear previous feedback

        for (int i = 0; i < answerButtons.Length; i++)
        {
            // Update button text
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.answers[i];

            // Re-enable button
            answerButtons[i].interactable = true;

            // Remove old listeners to avoid stacking calls
            answerButtons[i].onClick.RemoveAllListeners();

            // Add new listener (closure fix)
            int index = i;
            answerButtons[i].onClick.AddListener(() => OnAnswerClicked(index));
        }
    }

    void OnAnswerClicked(int index)
    {
        if (isQuizFinished) return;

        Question q = questions[currentQuestionIndex];

        if (index == q.correctAnswerIndex)
        {
            score++;
            feedbackText.text = "<color=green>Correct!</color>";
        }
        else
        {
            feedbackText.text = "<color=red>Wrong!</color> The answer was: " + q.answers[q.correctAnswerIndex];
        }

        // Disable buttons so user can't click twice
        foreach (var btn in answerButtons) btn.interactable = false;

        // Wait 2 seconds then go to next question
        Invoke("NextQuestion", 2f);
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        ShowQuestion();
    }

    void FinishQuiz()
    {
        isQuizFinished = true;
        questionText.text = "Quiz Completed!";
        feedbackText.text = $"Your Score: {score} / {questions.Count}";

        // Hide answer buttons
        foreach (var btn in answerButtons) btn.gameObject.SetActive(false);

        // Show restart button
        restartButton.SetActive(true);
    }

    public void RestartQuiz()
    {
        // Simple way: reload the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}