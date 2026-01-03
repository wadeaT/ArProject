using UnityEngine;
using TMPro;

public class ForceVisualizer : MonoBehaviour
{
    public ArmTracker armTracker;

    [Header("Force Arrows")]
    private ForceArrow resistanceArrow;       // Weight (down) or Cable (up)
    private ForceArrow forearmWeightArrow;    // Always down (gravity)
    private ForceArrow muscleForceArrow;      // Biceps or Triceps
    private ForceArrow jointReactionArrow;    // Joint reaction force

    [Header("Labels")]
    private TextMeshPro resistanceLabel;
    private TextMeshPro forearmWeightLabel;
    private TextMeshPro muscleForceLabel;
    private TextMeshPro jointReactionLabel;

    [Header("Visualization Settings")]
    public float weightForceScale = 0.005f;
    public float muscleForceScale = 0.0005f;

    [Header("Colors")]
    public Color weightColor = Color.red;           // Gravity/weight pulling down
    public Color cableColor = Color.blue;           // Cable tension pulling up
    public Color forearmWeightColor = Color.yellow; // Forearm weight (always gravity)
    public Color bicepsColor = Color.green;
    public Color tricepsColor = new Color(0.7f, 0.3f, 1f); // Purple
    public Color jointReactionColor = Color.gray;

    private bool showJointReaction = false;

    void Start()
    {
        // Create force arrows
        resistanceArrow = CreateArrow("ResistanceArrow", weightColor);
        forearmWeightArrow = CreateArrow("ForearmWeightArrow", forearmWeightColor);
        muscleForceArrow = CreateArrow("MuscleForceArrow", bicepsColor);
        jointReactionArrow = CreateArrow("JointReactionArrow", jointReactionColor);

        // Create labels
        resistanceLabel = CreateLabel("Resistance");
        forearmWeightLabel = CreateLabel("Forearm Weight");
        muscleForceLabel = CreateLabel("Muscle Force");
        jointReactionLabel = CreateLabel("Joint Reaction");
    }

    void Update()
    {
        if (armTracker != null && armTracker.AllTracked())
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

        ArmTracker.MuscleMode mode = armTracker.GetCurrentMuscleMode();

        // ════════════════════════════════════════════════════════════════
        // 1. RESISTANCE FORCE (Weight DOWN for biceps, Cable UP for triceps)
        // ════════════════════════════════════════════════════════════════
        float resistanceForce = armTracker.GetResistanceForce();
        Vector3 resistanceDirection = armTracker.GetResistanceForceDirection();

        // Change color and label based on mode
        if (mode == ArmTracker.MuscleMode.Biceps)
        {
            // Weight pulling DOWN (gravity)
            resistanceArrow.SetColor(weightColor);
            resistanceArrow.DrawArrow(handPos, resistanceDirection, resistanceForce, weightForceScale);

            Vector3 resistanceLabelPos = handPos + resistanceDirection * 0.1f;
            resistanceLabel.color = weightColor;
            UpdateLabel(resistanceLabel, resistanceLabelPos,
                $"WEIGHT\n{resistanceForce:F1} N\n↓ Gravity");
        }
        else // Triceps
        {
            // Cable pulling UP
            resistanceArrow.SetColor(cableColor);
            resistanceArrow.DrawArrow(handPos, resistanceDirection, resistanceForce, weightForceScale);

            Vector3 resistanceLabelPos = handPos + resistanceDirection * 0.1f;
            resistanceLabel.color = cableColor;
            UpdateLabel(resistanceLabel, resistanceLabelPos,
                $"CABLE\n{resistanceForce:F1} N\n↑ Tension");
        }

        // ════════════════════════════════════════════════════════════════
        // 2. FOREARM WEIGHT (always DOWN - gravity doesn't change!)
        // ════════════════════════════════════════════════════════════════
        float forearmWeightForce = armTracker.GetForearmWeightForce();
        forearmWeightArrow.DrawArrow(forearmCenter, Vector3.down, forearmWeightForce, weightForceScale);

        Vector3 forearmLabelPos = forearmCenter + Vector3.down * 0.08f;
        UpdateLabel(forearmWeightLabel, forearmLabelPos,
            $"Arm Weight\n{forearmWeightForce:F1} N");

        // ════════════════════════════════════════════════════════════════
        // 3. MUSCLE FORCE (direction from ArmTracker)
        // ════════════════════════════════════════════════════════════════
        float muscleForce = armTracker.GetMuscleForce();
        Vector3 muscleDirection = armTracker.GetMuscleForceDirection();
        Vector3 muscleInsertionPoint = armTracker.GetMuscleInsertionPoint();

        // Update color based on muscle mode
        Color muscleColor = (mode == ArmTracker.MuscleMode.Biceps) ? bicepsColor : tricepsColor;
        muscleForceArrow.SetColor(muscleColor);

        muscleForceArrow.DrawArrow(muscleInsertionPoint, muscleDirection, muscleForce, muscleForceScale);

        // Label
        string muscleName = (mode == ArmTracker.MuscleMode.Biceps) ? "BICEPS" : "TRICEPS";
        string muscleAction = (mode == ArmTracker.MuscleMode.Biceps) ? "Flexion" : "Extension";
        Vector3 muscleLabelPos = muscleInsertionPoint + muscleDirection * 0.08f;
        muscleForceLabel.color = muscleColor;
        UpdateLabel(muscleForceLabel, muscleLabelPos,
            $"{muscleName}\n{muscleForce:F0} N\n({muscleAction})");

        // ════════════════════════════════════════════════════════════════
        // 4. JOINT REACTION FORCE (Advanced mode only)
        // ════════════════════════════════════════════════════════════════
        if (showJointReaction)
        {
            DrawJointReactionForce(elbowPos, resistanceForce, resistanceDirection,
                                   forearmWeightForce, muscleForce, muscleDirection);
        }
        else
        {
            jointReactionArrow.SetVisibility(false);
            if (jointReactionLabel != null) jointReactionLabel.gameObject.SetActive(false);
        }
    }

    void DrawJointReactionForce(Vector3 elbowPos, float resistanceForce, Vector3 resistanceDir,
                                 float forearmWeight, float muscleForce, Vector3 muscleDir)
    {
        // ════════════════════════════════════════════════════════════════
        // JOINT REACTION FORCE (for complete FBD)
        // ════════════════════════════════════════════════════════════════
        // For force equilibrium: Σ F = 0
        // F_joint + F_muscle + F_resistance + W_forearm = 0
        // F_joint = -(F_muscle + F_resistance + W_forearm)
        // ════════════════════════════════════════════════════════════════

        Vector3 muscleForceVec = muscleDir * muscleForce;
        Vector3 resistanceForceVec = resistanceDir * resistanceForce;
        Vector3 forearmWeightVec = Vector3.down * forearmWeight;

        Vector3 jointReactionVec = -(muscleForceVec + resistanceForceVec + forearmWeightVec);
        float jointReactionMagnitude = jointReactionVec.magnitude;
        Vector3 jointReactionDir = jointReactionVec.normalized;

        jointReactionArrow.DrawArrow(elbowPos, jointReactionDir, jointReactionMagnitude, muscleForceScale);

        Vector3 jointLabelPos = elbowPos + jointReactionDir * 0.06f;
        jointReactionLabel.color = jointReactionColor;
        UpdateLabel(jointReactionLabel, jointLabelPos,
            $"Joint Force\n{jointReactionMagnitude:F0} N");
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
        resistanceArrow?.SetVisibility(false);
        forearmWeightArrow?.SetVisibility(false);
        muscleForceArrow?.SetVisibility(false);
        jointReactionArrow?.SetVisibility(false);

        if (resistanceLabel != null) resistanceLabel.gameObject.SetActive(false);
        if (forearmWeightLabel != null) forearmWeightLabel.gameObject.SetActive(false);
        if (muscleForceLabel != null) muscleForceLabel.gameObject.SetActive(false);
        if (jointReactionLabel != null) jointReactionLabel.gameObject.SetActive(false);
    }

    // ════════════════════════════════════════════════════════════════
    // PUBLIC METHOD FOR TOGGLE - Called by ViewModeController
    // ════════════════════════════════════════════════════════════════
    public void ToggleJointReaction(bool isOn)
    {
        showJointReaction = isOn;
        Debug.Log($"Joint Reaction Force toggled: {isOn}");
    }
}