using UnityEngine;

public class ForceVisualizer : MonoBehaviour
{
    public ArmTracker armTracker;

    [Header("Force Arrows")]
    private ForceArrow resistanceArrow;
    private ForceArrow forearmWeightArrow;
    private ForceArrow muscleForceArrow;
    private ForceArrow jointReactionArrow;

    [Header("Visualization Settings")]
    public float weightForceScale = 0.005f;
    public float muscleForceScale = 0.0005f;

    [Header("Colors")]
    public Color weightColor = Color.red;
    public Color cableColor = Color.blue;
    public Color forearmWeightColor = Color.yellow;
    public Color bicepsColor = Color.green;
    public Color tricepsColor = new Color(0.7f, 0.3f, 1f);
    public Color jointReactionColor = Color.gray;

    private bool showJointReaction = false;

    // Cached values for external access
    private float cachedResistanceForce;
    private float cachedForearmWeight;
    private float cachedMuscleForce;
    private float cachedJointForce;
    private Color cachedResistanceColor;
    private Color cachedMuscleColor;

    void Start()
    {
        resistanceArrow = CreateArrow("ResistanceArrow", weightColor);
        forearmWeightArrow = CreateArrow("ForearmWeightArrow", forearmWeightColor);
        muscleForceArrow = CreateArrow("MuscleForceArrow", bicepsColor);
        jointReactionArrow = CreateArrow("JointReactionArrow", jointReactionColor);
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

        // 1. RESISTANCE FORCE
        cachedResistanceForce = armTracker.GetResistanceForce();
        Vector3 resistanceDirection = armTracker.GetResistanceForceDirection();
        cachedResistanceColor = (mode == ArmTracker.MuscleMode.Biceps) ? weightColor : cableColor;

        resistanceArrow.SetColor(cachedResistanceColor);
        resistanceArrow.DrawArrow(handPos, resistanceDirection, cachedResistanceForce, weightForceScale);

        // 2. FOREARM WEIGHT
        cachedForearmWeight = armTracker.GetForearmWeightForce();
        forearmWeightArrow.SetColor(forearmWeightColor);
        forearmWeightArrow.DrawArrow(forearmCenter, Vector3.down, cachedForearmWeight, weightForceScale);

        // 3. MUSCLE FORCE
        cachedMuscleForce = armTracker.GetMuscleForce();
        Vector3 muscleDirection = armTracker.GetMuscleForceDirection();
        Vector3 muscleInsertionPoint = armTracker.GetMuscleInsertionPoint();

        cachedMuscleColor = (mode == ArmTracker.MuscleMode.Biceps) ? bicepsColor : tricepsColor;
        muscleForceArrow.SetColor(cachedMuscleColor);
        muscleForceArrow.DrawArrow(muscleInsertionPoint, muscleDirection, cachedMuscleForce, muscleForceScale);

        // 4. JOINT REACTION FORCE
        if (showJointReaction)
        {
            Vector3 muscleForceVec = muscleDirection * cachedMuscleForce;
            Vector3 resistanceForceVec = resistanceDirection * cachedResistanceForce;
            Vector3 forearmWeightVec = Vector3.down * cachedForearmWeight;

            Vector3 jointReactionVec = -(muscleForceVec + resistanceForceVec + forearmWeightVec);
            cachedJointForce = jointReactionVec.magnitude;
            Vector3 jointReactionDir = jointReactionVec.normalized;

            jointReactionArrow.SetColor(jointReactionColor);
            jointReactionArrow.DrawArrow(elbowPos, jointReactionDir, cachedJointForce, muscleForceScale);
        }
        else
        {
            jointReactionArrow.SetVisibility(false);
            cachedJointForce = 0f;
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

    void HideAllArrows()
    {
        resistanceArrow?.SetVisibility(false);
        forearmWeightArrow?.SetVisibility(false);
        muscleForceArrow?.SetVisibility(false);
        jointReactionArrow?.SetVisibility(false);
    }

    public void ToggleJointReaction(bool isOn)
    {
        showJointReaction = isOn;
    }

    // PUBLIC GETTERS
    public float GetResistanceForce() => cachedResistanceForce;
    public float GetForearmWeightForce() => cachedForearmWeight;
    public float GetMuscleForce() => cachedMuscleForce;
    public float GetJointForce() => cachedJointForce;
    public bool IsJointReactionVisible() => showJointReaction;

    public Color GetResistanceColor() => cachedResistanceColor;
    public Color GetForearmWeightColor() => forearmWeightColor;
    public Color GetMuscleColor() => cachedMuscleColor;
    public Color GetJointReactionColor() => jointReactionColor;
}