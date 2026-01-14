using System.Collections.Generic;

[System.Serializable]
public class Question
{
    public string questionText;
    public string[] answers;
    public int correctAnswerIndex;
}

public static class QuizData
{
    public static List<Question> GetQuestions()
    {
        return new List<Question>()
        {
            // Question 1: Distance / Torque
            new Question()
            {
                questionText = "If you hold a weight in your hand, when is it hardest to hold?",
                answers = new string[] {
                    "When your arm is fully stretched out",
                    "When your arm is close to your body",
                    "It feels the same either way",
                    "When you drop the weight"
                },
                correctAnswerIndex = 0
            },

            // Question 2: Pivot Identification
            new Question()
            {
                questionText = "In the arm system you just saw, which part acts as the 'Pivot' (rotation point)?",
                answers = new string[] {
                    "The Hand",
                    "The Elbow Joint",
                    "The Bicep Muscle",
                    "The Weight"
                },
                correctAnswerIndex = 1
            },

            // --- CHANGED QUESTION 3 ---
            // Focus: Simple comparison (Muscle vs Weight)
            new Question()
            {
                questionText = "To lift a 5kg weight, does your bicep muscle need to pull with MORE force or LESS force?",
                answers = new string[] {
                    "Much MORE than 5kg",
                    "Less than 5kg",
                    "Exactly 5kg",
                    "Zero force"
                },
                correctAnswerIndex = 0
            },

            // --- CHANGED QUESTION 4 ---
            // Focus: Mechanical Advantage (Changing the design)
            new Question()
            {
                questionText = "If you could attach your muscle further away from your elbow, what would happen?",
                answers = new string[] {
                    "It would be harder to lift the weight",
                    "It would be easier to lift the weight",
                    "The weight would disappear",
                    "The arm would break"
                },
                correctAnswerIndex = 1
            }
        };
    }
}