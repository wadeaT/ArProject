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
        // Get current muscle mode
        string modeHeader = "";
        string modeExplanation = "";

        if (armTracker != null)
        {
            ArmTracker.MuscleMode mode = armTracker.GetCurrentMuscleMode();

            if (mode == ArmTracker.MuscleMode.Biceps)
            {
                modeHeader = "<color=green>💪 BICEPS CURL</color>";
                modeExplanation =
                    "<color=red>Red</color> = Weight (↓ gravity)\n" +
                    "<color=green>Green</color> = Biceps pulls UP\n" +
                    "Action: Elbow FLEXION";
            }
            else
            {
                modeHeader = "<color=#AA55FF>💪 TRICEPS PULLDOWN</color>";
                modeExplanation =
                    "<color=#5555FF>Blue</color> = Cable (↑ tension)\n" +
                    "<color=#AA55FF>Purple</color> = Triceps pulls UP\n" +
                    "Action: Elbow EXTENSION";
            }
        }

        switch (modeController.currentMode)
        {
            case ViewModeController.ViewMode.Basic:
                infoText.text = "💡 <b>Forces & Balance</b>\n" +
                               modeHeader + "\n\n" +
                               modeExplanation + "\n\n" +
                               "<color=yellow>Yellow</color> = Arm weight (↓)\n\n" +
                               "⚖️ Equilibrium: Σ τ = 0";
                break;

            case ViewModeController.ViewMode.TorqueAnalysis:
                infoText.text = "💡 <b>Torque Analysis</b>\n" +
                               modeHeader + "\n\n" +
                               "<color=cyan>Cyan</color> = Moment arms (r⊥)\n\n" +
                               "τ = F × r⊥\n\n" +
                               "🔍 <b>Key insight:</b>\n" +
                               "Muscle r⊥ ≈ 5 cm (small!)\n" +
                               "Load r⊥ ≈ 30 cm (large!)\n\n" +
                               "→ Muscle force is ~6× the load!";
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
                        resistanceNote = "Cable pulls UP, Arm weight\npulls DOWN (oppose each other)";
                    }
                }

                infoText.text = "💡 <b>Complete Analysis</b>\n" +
                               modeHeader + "\n\n" +
                               "<b>4 Forces (FBD):</b>\n" +
                               "1. Resistance (hand)\n" +
                               "2. Forearm weight\n" +
                               "3. Muscle force\n" +
                               "4. Joint reaction\n\n" +
                               "<b>Note:</b>\n" +
                               resistanceNote;
                break;
        }
    }
}