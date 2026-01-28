using UnityEngine;
using TMPro;

/// <summary>
/// Displays physics values with colors matching the 3D arrows.
/// Create your own panel in Unity and assign the text references.
/// </summary>
public class PhysicsDataPanel : MonoBehaviour
{
    [Header("References")]
    public ArmTracker armTracker;
    public ForceVisualizer forceVisualizer;
    public MomentArmVisualizer momentArmVisualizer;

    [Header("Force Texts")]
    public TMP_Text resistanceText;
    public TMP_Text forearmWeightText;
    public TMP_Text muscleForceText;
    public TMP_Text jointForceText;

    [Header("Torque Text")]
    public TMP_Text torqueText;

    [Header("Moment Arm Texts")]
    public TMP_Text handMomentArmText;
    public TMP_Text forearmMomentArmText;
    public TMP_Text muscleMomentArmText;

    void Update()
    {
        if (armTracker == null || !armTracker.AllTracked()) return;

        UpdateForces();
        UpdateTorque();
        UpdateMomentArms();
    }

    void UpdateForces()
    {
        if (forceVisualizer == null) return;

        ArmTracker.MuscleMode mode = armTracker.GetCurrentMuscleMode();

        // 1. RESISTANCE FORCE (Red for weight, Blue for cable)
        if (resistanceText != null)
        {
            string label = (mode == ArmTracker.MuscleMode.Biceps) ? "W" : "T";
            resistanceText.text = $"{label} = {forceVisualizer.GetResistanceForce():F1} N";
            resistanceText.color = forceVisualizer.GetResistanceColor();  // ← ADDED COLOR
        }

        // 2. FOREARM WEIGHT (Yellow)
        if (forearmWeightText != null)
        {
            forearmWeightText.text = $"W-arm = {forceVisualizer.GetForearmWeightForce():F1} N";
            forearmWeightText.color = forceVisualizer.GetForearmWeightColor();  // ← ADDED COLOR
        }

        // 3. MUSCLE FORCE (Green for biceps, Purple for triceps)
        if (muscleForceText != null)
        {
            string label = (mode == ArmTracker.MuscleMode.Biceps) ? "F-biceps" : "F-triceps";
            muscleForceText.text = $"{label} = {forceVisualizer.GetMuscleForce():F0} N";
            muscleForceText.color = forceVisualizer.GetMuscleColor();  // ← ADDED COLOR
        }

        // 4. JOINT FORCE (Gray)
        if (jointForceText != null)
        {
            if (forceVisualizer.IsJointReactionVisible())
            {
                jointForceText.gameObject.SetActive(true);
                jointForceText.text = $"F-joint = {forceVisualizer.GetJointForce():F0} N";
                jointForceText.color = forceVisualizer.GetJointReactionColor();  // ← ADDED COLOR
            }
            else
            {
                jointForceText.gameObject.SetActive(false);
            }
        }
    }

    void UpdateTorque()
    {
        if (torqueText != null && armTracker != null)
        {
            torqueText.text = $"τ = {armTracker.GetElbowTorque():F2} N·m";
            torqueText.color = new Color(1f, 0.84f, 0f);  // Gold color (matches torque arrow)
        }
    }

    void UpdateMomentArms()
    {
        if (momentArmVisualizer == null) return;

        bool visible = momentArmVisualizer.IsVisible();

        // Moment arm colors (Cyan for weight arms, Green for muscle arm)
        Color weightArmColor = new Color(0f, 1f, 1f);  // Cyan
        Color muscleArmColor = new Color(0f, 1f, 0f);  // Green

        // 1. HAND MOMENT ARM (Cyan)
        if (handMomentArmText != null)
        {
            handMomentArmText.gameObject.SetActive(visible);
            if (visible)
            {
                handMomentArmText.text = $"r-hand = {momentArmVisualizer.GetHandMomentArm() * 100:F1} cm";
                handMomentArmText.color = weightArmColor;  // ← ADDED COLOR
            }
        }

        // 2. FOREARM MOMENT ARM (Cyan)
        if (forearmMomentArmText != null)
        {
            forearmMomentArmText.gameObject.SetActive(visible);
            if (visible)
            {
                forearmMomentArmText.text = $"r-arm = {momentArmVisualizer.GetForearmMomentArm() * 100:F1} cm";
                forearmMomentArmText.color = weightArmColor;  // ← ADDED COLOR
            }
        }

        // 3. MUSCLE MOMENT ARM (Green)
        if (muscleMomentArmText != null)
        {
            muscleMomentArmText.gameObject.SetActive(visible);
            if (visible)
            {
                muscleMomentArmText.text = $"r-muscle = {momentArmVisualizer.GetMuscleMomentArm() * 100:F1} cm";
                muscleMomentArmText.color = muscleArmColor;  // ← ADDED COLOR
            }
        }
    }
}