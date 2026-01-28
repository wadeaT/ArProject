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
            // Question 1: Moment Arm / Torque (Distance)
            new Question()
            {
                questionText = "If you hold a weight in your hand, when is it hardest to hold?",
                answers = new string[] {
                    "When your arm is fully stretched out", // Correct: Max distance from pivot = Max Torque
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
                    "The Elbow Joint", // Correct: The axis of rotation
                    "The Bicep Muscle",
                    "The Weight"
                },
                correctAnswerIndex = 1
            },

            // Question 3: Force Magnitude (Mechanical Disadvantage)
            new Question()
            {
                questionText = "To lift a 5kg weight, does your bicep muscle need to pull with MORE force or LESS force?",
                answers = new string[] {
                    "Much MORE than 5kg", // Correct: Because the muscle is close to the pivot
                    "Less than 5kg",
                    "Exactly 5kg",
                    "Zero force"
                },
                correctAnswerIndex = 0
            },

            // Question 4: Mechanical Advantage (Lever Arm)
            new Question()
            {
                questionText = "If you could attach your muscle further away from your elbow, what would happen?",
                answers = new string[] {
                    "It would be harder to lift the weight",
                    "It would be easier to lift the weight", // Correct: Longer lever arm = More Torque
                    "The weight would disappear",
                    "The arm would break"
                },
                correctAnswerIndex = 1
            },

            // Question 5: Simple Machine Classification
            new Question()
            {
                questionText = "In physics, the human arm acts like which simple machine?",
                answers = new string[] {
                    "A Pulley",
                    "A Lever", // Correct: Specifically a Class 3 Lever
                    "A Spring",
                    "A Wedge"
                },
                correctAnswerIndex = 1
            }
        };
    }
}