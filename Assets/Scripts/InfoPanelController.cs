using UnityEngine;
using TMPro;

public class InfoPanelController : MonoBehaviour
{
    public TMP_Text infoText;
    public ViewModeController modeController;

    void Update()
    {
        if (modeController != null && infoText != null)
        {
            UpdateInfoText();
        }
    }

    void UpdateInfoText()
    {
        switch (modeController.currentMode)
        {
            case ViewModeController.ViewMode.Basic:
                infoText.text =
                    " <b>Forces & Balance</b>\n\n" +
                    "<color=#FF6B6B>Red</color> = Weights (gravity pulls down)\n" +
                    "<color=#4ECDC4>Yellow</color> = Arm weight\n" +
                    "<color=#95E1D3>Green</color> = Muscle force (bicep pulls up)\n\n" +
                    "<b>Try This:</b>\n" +
                    "• Change the weight - what happens?\n" +
                    "• Why is the muscle force SO large?\n" +
                    "• Move the arm - does muscle force change?";
                break;

            case ViewModeController.ViewMode.TorqueAnalysis:
                infoText.text =
                    " <b>Lever & Torque</b>\n\n" +
                    "<color=cyan>Cyan lines</color> = lever arms (r⊥)\n" +
                    "Distance from elbow to force\n" +
                    "<color=yellow>Yellow curve</color> = rotation direction\n\n" +
                    "<b>Investigate:</b>\n" +
                    "• Compare the two lever arm lengths\n" +
                    "• Torque = Force × Distance\n" +
                    "• Which creates more torque?\n" +
                    "• Move arm up/down - what changes?";
                break;

            case ViewModeController.ViewMode.Advanced:
                infoText.text =
                    " <b>Complete Analysis</b>\n\n" +
                    "<color=red>XYZ</color> axes show 3D space\n" +
                    "<color=yellow>Gold arrow</color> = torque vector\n" +
                    "  (points along rotation axis)\n" +
                    "<color=gray>Gray arrow</color> = joint reaction\n" +
                    "  (zero torque at pivot)\n\n" +
                    "<b>Advanced Challenge:</b>\n" +
                    "• Use right-hand rule for torque\n" +
                    "• All forces must balance (ΣF = 0)\n" +
                    "• All torques must balance (Στ = 0)";
                break;
        }
    }
}