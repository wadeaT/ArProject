using UnityEngine;
using TMPro;

public class ForceVisualizer : MonoBehaviour
{
    public ArmTracker armTracker;

    [Header("Force Arrows")]
    private ForceArrow handWeightArrow;
    private ForceArrow forearmWeightArrow;
    private ForceArrow muscleForceArrow;
    private ForceArrow jointReactionArrow; // NEW: 4th force for Advanced mode

    [Header("Labels")]
    private TextMeshPro handWeightLabel;
    private TextMeshPro forearmWeightLabel;
    private TextMeshPro muscleForceLabel;
    private TextMeshPro jointReactionLabel; // NEW

    [Header("Mode Control")]
    private bool showJointReaction = false; // Controlled by ViewModeController

    void Start()
    {
        // Create force arrows
        handWeightArrow = CreateArrow("HandWeightArrow", Color.red);
        forearmWeightArrow = CreateArrow("ForearmWeightArrow", Color.yellow);
        muscleForceArrow = CreateArrow("MuscleForceArrow", Color.green);
        jointReactionArrow = CreateArrow("JointReactionArrow", new Color(0.5f, 0.5f, 0.5f, 0.7f)); // Gray

        // Create labels
        handWeightLabel = CreateLabel("Hand Weight");
        forearmWeightLabel = CreateLabel("Forearm Weight");
        muscleForceLabel = CreateLabel("Muscle Force");
        jointReactionLabel = CreateLabel("Joint Reaction");
    }

    void Update()
    {
        if (armTracker.AllTracked())
        {
            DrawForces();
        }
        else
        {
            HideAllArrows();
        }
    }

    void DrawForces()
    {
        Vector3 elbowPos = armTracker.GetElbowPos();
        Vector3 handPos = armTracker.GetHandPos();
        Vector3 forearmCenter = (elbowPos + handPos) / 2f;

        // Scale factors: smaller forces get bigger scale so they're visible
        float weightScale = 0.005f;  // Weights are small, so bigger scale
        float muscleScale = 0.001f;  // Muscle force is huge, so smaller scale

        // ═════════════════════════════════════════════════════════
        // 1. HAND WEIGHT (downward from hand)
        // ═════════════════════════════════════════════════════════
        float handWeightForce = armTracker.GetHandWeightForce();
        handWeightArrow.DrawArrow(handPos, Vector3.down, handWeightForce, weightScale);
        UpdateLabel(handWeightLabel, handPos + Vector3.down * 0.08f,
            $"Hand Weight\n{handWeightForce:F1} N");

        // ═════════════════════════════════════════════════════════
        // 2. FOREARM WEIGHT (downward from center of forearm)
        // ═════════════════════════════════════════════════════════
        float forearmWeightForce = armTracker.GetForearmWeightForce();
        forearmWeightArrow.DrawArrow(forearmCenter, Vector3.down, forearmWeightForce, weightScale);
        UpdateLabel(forearmWeightLabel, forearmCenter + Vector3.down * 0.08f,
            $"Arm Weight\n{forearmWeightForce:F1} N");

        // ═════════════════════════════════════════════════════════
        // 3. MUSCLE FORCE (upward from bicep insertion point)
        // IMPROVED: Now draws at actual insertion point, not elbow
        // ═════════════════════════════════════════════════════════
        float muscleForce = armTracker.GetMuscleForce();
        Vector3 bicepInsertionPoint = armTracker.GetBicepInsertionPoint();
        muscleForceArrow.DrawArrow(bicepInsertionPoint, Vector3.up, muscleForce, muscleScale);
        UpdateLabel(muscleForceLabel, bicepInsertionPoint + Vector3.up * 0.08f,
            $"BICEP FORCE\n{muscleForce:F1} N");

        // ═════════════════════════════════════════════════════════
        // 4. JOINT REACTION FORCE (only in Advanced mode)
        // This is the force the upper arm exerts on the forearm at elbow
        // For vertical equilibrium: F_joint + F_muscle = W_hand + W_arm
        // ═════════════════════════════════════════════════════════
        if (showJointReaction)
        {
            // Calculate joint reaction force (balances all other vertical forces)
            float totalDownwardForce = handWeightForce + forearmWeightForce;
            float jointReactionForce = totalDownwardForce - muscleForce;

            // Joint reaction typically points upward (positive) if muscle force isn't enough
            // or downward (negative) if muscle force is too much
            Vector3 jointDirection = jointReactionForce > 0 ? Vector3.up : Vector3.down;
            float jointMagnitude = Mathf.Abs(jointReactionForce);

            jointReactionArrow.DrawArrow(elbowPos, jointDirection, jointMagnitude, weightScale);
            UpdateLabel(jointReactionLabel, elbowPos + jointDirection * 0.1f,
                $"Joint Reaction\n{jointMagnitude:F1} N\n(τ = 0 at pivot)");
        }
        else
        {
            jointReactionArrow.SetVisibility(false);
            if (jointReactionLabel != null)
                jointReactionLabel.gameObject.SetActive(false);
        }
    }

    ForceArrow CreateArrow(string name, Color color)
    {
        GameObject arrowObj = new GameObject(name);
        arrowObj.transform.SetParent(transform);
        ForceArrow arrow = arrowObj.AddComponent<ForceArrow>();
        arrow.SetColor(color);
        return arrow;
    }

    TextMeshPro CreateLabel(string initialText)
    {
        GameObject labelObj = new GameObject($"Label_{initialText}");
        labelObj.transform.SetParent(transform);

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = initialText;
        tmp.fontSize = 0.3f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Make it face camera
        labelObj.AddComponent<Billboard>();

        return tmp;
    }

    void UpdateLabel(TextMeshPro label, Vector3 position, string text)
    {
        if (label != null)
        {
            label.transform.position = position;
            label.text = text;
            label.gameObject.SetActive(true);
        }
    }

    void HideAllArrows()
    {
        handWeightArrow?.SetVisibility(false);
        forearmWeightArrow?.SetVisibility(false);
        muscleForceArrow?.SetVisibility(false);
        jointReactionArrow?.SetVisibility(false);

        if (handWeightLabel != null) handWeightLabel.gameObject.SetActive(false);
        if (forearmWeightLabel != null) forearmWeightLabel.gameObject.SetActive(false);
        if (muscleForceLabel != null) muscleForceLabel.gameObject.SetActive(false);
        if (jointReactionLabel != null) jointReactionLabel.gameObject.SetActive(false);
    }

    // ═════════════════════════════════════════════════════════════════
    // PUBLIC METHOD: Toggle joint reaction force (called by ViewModeController)
    // ═════════════════════════════════════════════════════════════════
    public void ToggleJointReaction(bool isOn)
    {
        showJointReaction = isOn;
        Debug.Log($"Joint Reaction Force toggled: {isOn}");
    }
}