using UnityEngine;

public class ViewModeController : MonoBehaviour
{
    [Header("Visualizer References")]
    public MomentArmVisualizer momentArmViz;
    public CoordinateAxes coordinateAxes;
    public TorqueVectorVisualizer torqueVectorViz;
    public CurvedTorqueArrow curvedTorqueArrow;
    public ForceVisualizer forceVisualizer; // NEW: For joint reaction toggle

    [Header("Current Mode")]
    public ViewMode currentMode = ViewMode.Basic;

    public enum ViewMode
    {
        Basic,           // "Forces & Balance"
        TorqueAnalysis,  // "Lever & Torque"
        Advanced         // "Complete Analysis"
    }

    void Start()
    {
        // Set initial mode
        SetMode(currentMode);
    }

    // ═════════════════════════════════════════════════════════════════
    // PUBLIC METHODS (called by UI toggles)
    // ═════════════════════════════════════════════════════════════════
    public void SetBasicMode()
    {
        SetMode(ViewMode.Basic);
    }

    public void SetTorqueMode()
    {
        SetMode(ViewMode.TorqueAnalysis);
    }

    public void SetAdvancedMode()
    {
        SetMode(ViewMode.Advanced);
    }

    // ═════════════════════════════════════════════════════════════════
    // CORE MODE SWITCHING LOGIC
    // ═════════════════════════════════════════════════════════════════
    void SetMode(ViewMode mode)
    {
        currentMode = mode;

        switch (mode)
        {
            case ViewMode.Basic:
                // Only show basic force arrows (3 forces: hand, arm, muscle)
                if (momentArmViz != null) momentArmViz.ToggleMomentArms(false);
                if (coordinateAxes != null) coordinateAxes.ToggleAxes(false);
                if (torqueVectorViz != null) torqueVectorViz.ToggleTorqueVector(false);
                if (curvedTorqueArrow != null) curvedTorqueArrow.ToggleCurvedArrow(false);
                if (forceVisualizer != null) forceVisualizer.ToggleJointReaction(false); // NEW
                Debug.Log("Mode: Forces & Balance (Basic)");
                break;

            case ViewMode.TorqueAnalysis:
                // Show moment arms and curved torque arrow
                if (momentArmViz != null) momentArmViz.ToggleMomentArms(true);
                if (coordinateAxes != null) coordinateAxes.ToggleAxes(false);
                if (torqueVectorViz != null) torqueVectorViz.ToggleTorqueVector(false);
                if (curvedTorqueArrow != null) curvedTorqueArrow.ToggleCurvedArrow(true);
                if (forceVisualizer != null) forceVisualizer.ToggleJointReaction(false); // NEW
                Debug.Log("Mode: Lever & Torque (Torque Analysis)");
                break;

            case ViewMode.Advanced:
                // Show everything including joint reaction force
                if (momentArmViz != null) momentArmViz.ToggleMomentArms(true);
                if (coordinateAxes != null) coordinateAxes.ToggleAxes(true);
                if (torqueVectorViz != null) torqueVectorViz.ToggleTorqueVector(true);
                if (curvedTorqueArrow != null) curvedTorqueArrow.ToggleCurvedArrow(true);
                if (forceVisualizer != null) forceVisualizer.ToggleJointReaction(true); // NEW
                Debug.Log("Mode: Complete Analysis (Advanced)");
                break;
        }
    }
}