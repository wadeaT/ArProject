using UnityEngine;
using TMPro;

/// <summary>
/// Simple script that updates text fields with physics values.
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

        if (resistanceText != null)
        {
            string label = (mode == ArmTracker.MuscleMode.Biceps) ? "W" : "T";
            resistanceText.text = $"{label} = {forceVisualizer.GetResistanceForce():F1} N";
        }

        if (forearmWeightText != null)
        {
            forearmWeightText.text = $"W-arm = {forceVisualizer.GetForearmWeightForce():F1} N";
        }

        if (muscleForceText != null)
        {
            string label = (mode == ArmTracker.MuscleMode.Biceps) ? "F-biceps" : "F-triceps";
            muscleForceText.text = $"{label} = {forceVisualizer.GetMuscleForce():F0} N";
        }

        if (jointForceText != null)
        {
            if (forceVisualizer.IsJointReactionVisible())
            {
                jointForceText.gameObject.SetActive(true);
                jointForceText.text = $"F-joint = {forceVisualizer.GetJointForce():F0} N";
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
            torqueText.text = $"T = {armTracker.GetElbowTorque():F2} N.m";
        }
    }

    void UpdateMomentArms()
    {
        if (momentArmVisualizer == null) return;

        bool visible = momentArmVisualizer.IsVisible();

        if (handMomentArmText != null)
        {
            handMomentArmText.gameObject.SetActive(visible);
            if (visible) handMomentArmText.text = $"r-hand = {momentArmVisualizer.GetHandMomentArm() * 100:F1} cm";
        }

        if (forearmMomentArmText != null)
        {
            forearmMomentArmText.gameObject.SetActive(visible);
            if (visible) forearmMomentArmText.text = $"r-arm = {momentArmVisualizer.GetForearmMomentArm() * 100:F1} cm";
        }

        if (muscleMomentArmText != null)
        {
            muscleMomentArmText.gameObject.SetActive(visible);
            if (visible) muscleMomentArmText.text = $"r-muscle = {momentArmVisualizer.GetMuscleMomentArm() * 100:F1} cm";
        }
    }
}