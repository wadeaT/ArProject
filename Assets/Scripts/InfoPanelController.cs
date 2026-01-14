using UnityEngine;
using TMPro;

public class InfoPanelController : MonoBehaviour
{
    [Header("References")]
    public TMP_Text infoText;
    public ViewModeController modeController;
    public ArmTracker armTracker;

    void Update()
    {
        if (modeController != null && infoText != null)
        {
            UpdateInfoText();
        }
    }

    void UpdateInfoText()
    {
        string modeHeader = "";
        string modeExplanation = "";

        if (armTracker != null)
        {
            ArmTracker.MuscleMode mode = armTracker.GetCurrentMuscleMode();

            if (mode == ArmTracker.MuscleMode.Biceps)
            {
                modeHeader = "BICEPS CURL";
                modeExplanation =
                    "Red = Weight (gravity)\n" +
                    "Green = Biceps pulls UP\n" +
                    "Yellow = Arm weight";
            }
            else
            {
                modeHeader = "TRICEPS PULLDOWN";
                modeExplanation =
                    "Blue = Cable (tension)\n" +
                    "Purple = Triceps pulls UP\n" +
                    "Yellow = Arm weight";
            }
        }

        switch (modeController.currentMode)
        {
            case ViewModeController.ViewMode.Basic:
                infoText.text = "Forces & Balance\n" +
                               modeHeader + "\n\n" +
                               modeExplanation;
                break;

            case ViewModeController.ViewMode.TorqueAnalysis:
                infoText.text = "Torque Analysis\n" +
                               modeHeader + "\n\n" +
                               "Cyan = Moment arms\n\n" +
                               "Key insight:\n" +
                               "Muscle arm = 5 cm (small)\n" +
                               "Load arm = 30 cm (large)\n\n" +
                               "Muscle force is 6x the load!";
                break;

            case ViewModeController.ViewMode.Advanced:
                string resistanceNote = "";
                if (armTracker != null)
                {
                    if (armTracker.GetCurrentMuscleMode() == ArmTracker.MuscleMode.Biceps)
                    {
                        resistanceNote = "Weight + Arm weight both\npull DOWN (add together)";
                    }
                    else
                    {
                        resistanceNote = "Cable pulls UP, Arm weight\npulls DOWN (oppose)";
                    }
                }

                infoText.text = "Complete Analysis\n" +
                               modeHeader + "\n\n" +
                               "4 Forces (FBD):\n" +
                               "1. Resistance (hand)\n" +
                               "2. Forearm weight\n" +
                               "3. Muscle force\n" +
                               "4. Joint reaction\n\n" +
                               resistanceNote;
                break;
        }
    }
}