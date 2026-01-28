using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI feedbackText;
    public GameObject restartButton;
    public GameObject Back2MainSceneButton;

    [Header("Button Colors")]
    public Color defaultButtonColor = Color.white;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

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
        feedbackText.text = "";

        for (int i = 0; i < answerButtons.Length; i++)
        {
            // Update button text
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.answers[i];

            // ═══════════════════════════════════════════════════════════════
            // FIX: Reset button color to default for each new question!
            // ═══════════════════════════════════════════════════════════════
            answerButtons[i].image.color = defaultButtonColor;

            // Re-enable button
            answerButtons[i].interactable = true;

            // Remove old listeners
            answerButtons[i].onClick.RemoveAllListeners();

            // Add new listener
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
            answerButtons[index].image.color = correctColor;
        }
        else
        {
            feedbackText.text = "<color=red>Wrong!</color> The answer was: " + q.answers[q.correctAnswerIndex];
            answerButtons[index].image.color = wrongColor;
            // Also highlight the correct answer
            answerButtons[q.correctAnswerIndex].image.color = correctColor;
        }

        foreach (var btn in answerButtons) btn.interactable = false;
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

        foreach (var btn in answerButtons) btn.gameObject.SetActive(false);
        restartButton.SetActive(true);
        Back2MainSceneButton.SetActive(true);
    }

    public void RestartQuiz()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

}