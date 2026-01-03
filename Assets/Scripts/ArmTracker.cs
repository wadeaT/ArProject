using UnityEngine;
using Vuforia;
using TMPro;

public class ArmTracker : MonoBehaviour
{
    [Header("Image Target References")]
    public ObserverBehaviour shoulderTarget;
    public ObserverBehaviour elbowTarget;
    public ObserverBehaviour handTarget;

    [Header("Debug Info")]
    public TMP_Text debugText;

    [Header("Weight Input")]
    public TMP_InputField weightInput;
    public float handWeight = 0f; // Weight/resistance in kg

    [Header("Arm Properties")]
    public float armMass = 2.0f; // Mass of forearm in kg

    [Header("Muscle Insertion Distances")]
    [Tooltip("Distance from elbow where biceps inserts (front of forearm)")]
    public float bicepInsertionDistance = 0.05f; // 5cm from elbow

    [Tooltip("Distance from elbow where triceps inserts (olecranon, back of elbow)")]
    public float tricepInsertionDistance = 0.02f; // 2cm behind elbow (olecranon)

    [Header("Muscle Mode")]
    public MuscleMode currentMuscleMode = MuscleMode.Biceps;

    public enum MuscleMode
    {
        Biceps,   // Lifting weight against gravity (curl)
        Triceps   // Pushing down against cable tension (pulldown)
    }

    [Header("Smoothing")]
    public float smoothingFactor = 0.3f;

    // Raw positions from tracking
    private Vector3 shoulderPos;
    private Vector3 elbowPos;
    private Vector3 handPos;

    // Smoothed positions
    private Vector3 smoothedShoulderPos;
    private Vector3 smoothedElbowPos;
    private Vector3 smoothedHandPos;

    private bool shoulderTracked = false;
    private bool elbowTracked = false;
    private bool handTracked = false;

    // Physics calculations
    private float elbowAngle;
    private float upperArmLength;
    private float forearmLength;

    // Torque values
    private float torqueFromResistance;      // Torque from hand weight/cable
    private float torqueFromForearmWeight;   // Torque from forearm mass (always gravity)
    private float totalLoadTorque;
    private float requiredMuscleTorque;
    private float requiredMuscleForce;

    // Moment arms
    private float handMomentArm;
    private float forearmMomentArm;
    private float muscleMomentArm;

    // Directions and positions for visualization
    private Vector3 muscleForceDirection;
    private Vector3 muscleInsertionPoint;
    private Vector3 resistanceForceDirection; // DOWN for biceps (gravity), UP for triceps (cable)

    private const float GRAVITY = 9.81f;

    void Start()
    {
        shoulderTarget.OnTargetStatusChanged += OnShoulderStatusChanged;
        elbowTarget.OnTargetStatusChanged += OnElbowStatusChanged;
        handTarget.OnTargetStatusChanged += OnHandStatusChanged;

        smoothedShoulderPos = Vector3.zero;
        smoothedElbowPos = Vector3.zero;
        smoothedHandPos = Vector3.zero;
    }

    void Update()
    {
        if (shoulderTracked)
        {
            shoulderPos = shoulderTarget.transform.position;
            smoothedShoulderPos = Vector3.Lerp(smoothedShoulderPos, shoulderPos, 1f - smoothingFactor);
        }

        if (elbowTracked)
        {
            elbowPos = elbowTarget.transform.position;
            smoothedElbowPos = Vector3.Lerp(smoothedElbowPos, elbowPos, 1f - smoothingFactor);
        }

        if (handTracked)
        {
            handPos = handTarget.transform.position;
            smoothedHandPos = Vector3.Lerp(smoothedHandPos, handPos, 1f - smoothingFactor);
        }

        if (shoulderTracked && elbowTracked && handTracked)
        {
            CalculateArmPhysics();
        }

        UpdateDebugText();
    }

    void CalculateArmPhysics()
    {
        upperArmLength = Vector3.Distance(smoothedShoulderPos, smoothedElbowPos);
        forearmLength = Vector3.Distance(smoothedElbowPos, smoothedHandPos);

        Vector3 upperArmDir = (smoothedElbowPos - smoothedShoulderPos).normalized;
        Vector3 forearmDir = (smoothedHandPos - smoothedElbowPos).normalized;
        elbowAngle = Vector3.Angle(upperArmDir, forearmDir);

        CalculateTorqueBalance();
    }

    void CalculateTorqueBalance()
    {
        Vector3 elbow = smoothedElbowPos;
        Vector3 hand = smoothedHandPos;
        Vector3 shoulder = smoothedShoulderPos;
        Vector3 forearmCenter = (elbow + hand) / 2f;

        Vector3 forearmDir = (hand - elbow).normalized;
        Vector3 upperArmDir = (shoulder - elbow).normalized;

        // Set resistance direction based on exercise type
        if (currentMuscleMode == MuscleMode.Biceps)
        {
            resistanceForceDirection = Vector3.down; // Gravity on dumbbell
        }
        else
        {
            resistanceForceDirection = Vector3.up; // Cable tension
        }

        // Calculate muscle insertion point and force direction
        if (currentMuscleMode == MuscleMode.Biceps)
        {
            muscleInsertionPoint = elbow + forearmDir * bicepInsertionDistance;
            muscleForceDirection = (shoulder - muscleInsertionPoint).normalized;
        }
        else
        {
            Vector3 backDirection = -forearmDir;
            muscleInsertionPoint = elbow + backDirection * tricepInsertionDistance;
            muscleForceDirection = (shoulder - muscleInsertionPoint).normalized;
        }

        // Moment arm calculations
        Vector3 elbowToHand = hand - elbow;

        if (currentMuscleMode == MuscleMode.Biceps)
        {
            handMomentArm = Mathf.Sqrt(elbowToHand.x * elbowToHand.x + elbowToHand.z * elbowToHand.z);
        }
        else
        {
            handMomentArm = Mathf.Sqrt(elbowToHand.x * elbowToHand.x + elbowToHand.z * elbowToHand.z);
        }

        Vector3 elbowToForearmCenter = forearmCenter - elbow;
        forearmMomentArm = Mathf.Sqrt(elbowToForearmCenter.x * elbowToForearmCenter.x +
                                       elbowToForearmCenter.z * elbowToForearmCenter.z);

        // Muscle moment arm using cross product
        Vector3 r_muscle = muscleInsertionPoint - elbow;
        Vector3 crossProduct = Vector3.Cross(r_muscle, muscleForceDirection);
        muscleMomentArm = crossProduct.magnitude;

        // Torque calculations
        float resistanceForce = handWeight * GRAVITY;
        float forearmWeightForce = armMass * GRAVITY;

        torqueFromResistance = resistanceForce * handMomentArm;
        torqueFromForearmWeight = forearmWeightForce * forearmMomentArm;

        // Total load torque depends on mode
        if (currentMuscleMode == MuscleMode.Biceps)
        {
            // Both forces cause extension torque
            totalLoadTorque = torqueFromResistance + torqueFromForearmWeight;
        }
        else
        {
            // Cable and gravity oppose each other
            totalLoadTorque = torqueFromResistance - torqueFromForearmWeight;
            totalLoadTorque = Mathf.Abs(totalLoadTorque);
        }

        // Required muscle force
        requiredMuscleTorque = totalLoadTorque;

        if (muscleMomentArm > 0.001f)
        {
            requiredMuscleForce = requiredMuscleTorque / muscleMomentArm;
        }
        else
        {
            requiredMuscleForce = 0f;
        }

        Debug.Log($"Mode: {currentMuscleMode}");
        Debug.Log($"Resistance Direction: {resistanceForceDirection}");
        Debug.Log($"Muscle Moment Arm: {muscleMomentArm:F3}m");
        Debug.Log($"Load Torque: {totalLoadTorque:F2} N⋅m");
        Debug.Log($"Muscle Force: {requiredMuscleForce:F1} N");
    }

    public void OnWeightUpdated()
    {
        if (float.TryParse(weightInput.text, out float weight))
        {
            handWeight = weight;
            Debug.Log($"Weight/Resistance updated to: {handWeight} kg");
        }
        else
        {
            Debug.LogWarning("Invalid weight input!");
        }
    }

    public void SetBicepsMode()
    {
        currentMuscleMode = MuscleMode.Biceps;
        Debug.Log("Switched to BICEPS mode (curl - weight pulls DOWN)");
    }

    public void SetTricepsMode()
    {
        currentMuscleMode = MuscleMode.Triceps;
        Debug.Log("Switched to TRICEPS mode (pulldown - cable pulls UP)");
    }

    void UpdateDebugText()
    {
        if (debugText != null)
        {
            string trackingStatus = $"Shoulder: {(shoulderTracked ? "YES" : "NO")}\n" +
                                   $"Elbow: {(elbowTracked ? "YES" : "NO")}\n" +
                                   $"Hand: {(handTracked ? "YES" : "NO")}\n\n";

            string physicsInfo = "";
            if (shoulderTracked && elbowTracked && handTracked)
            {
                string modeName = currentMuscleMode == MuscleMode.Biceps ? "BICEPS (Curl)" : "TRICEPS (Pulldown)";
                string resistanceType = currentMuscleMode == MuscleMode.Biceps ? "Weight ↓" : "Cable ↑";

                physicsInfo = $"Mode: {modeName}\n" +
                             $"Resistance: {resistanceType}\n" +
                             $"Elbow Angle: {elbowAngle:F1}°\n" +
                             $"Load: {handWeight:F1} kg\n" +
                             $"Torque: {totalLoadTorque:F2} N⋅m\n" +
                             $"Muscle Force: {requiredMuscleForce:F1} N";
            }

            debugText.text = trackingStatus + physicsInfo;
        }
    }

    // Tracking event handlers
    private void OnShoulderStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        shoulderTracked = (targetStatus.Status == Status.TRACKED ||
                          targetStatus.Status == Status.EXTENDED_TRACKED);
    }

    private void OnElbowStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        elbowTracked = (targetStatus.Status == Status.TRACKED ||
                       targetStatus.Status == Status.EXTENDED_TRACKED);
    }

    private void OnHandStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        handTracked = (targetStatus.Status == Status.TRACKED ||
                      targetStatus.Status == Status.EXTENDED_TRACKED);
    }

    void OnDestroy()
    {
        if (shoulderTarget != null)
            shoulderTarget.OnTargetStatusChanged -= OnShoulderStatusChanged;
        if (elbowTarget != null)
            elbowTarget.OnTargetStatusChanged -= OnElbowStatusChanged;
        if (handTarget != null)
            handTarget.OnTargetStatusChanged -= OnHandStatusChanged;
    }

    // ════════════════════════════════════════════════════════════════
    // PUBLIC GETTERS FOR VISUALIZATION
    // ════════════════════════════════════════════════════════════════

    // Positions
    public Vector3 GetShoulderPos() => smoothedShoulderPos;
    public Vector3 GetElbowPos() => smoothedElbowPos;
    public Vector3 GetHandPos() => smoothedHandPos;

    // Forces
    public float GetResistanceForce() => handWeight * GRAVITY;
    public float GetForearmWeightForce() => armMass * GRAVITY;
    public float GetMuscleForce() => requiredMuscleForce;

    // For backwards compatibility
    public float GetHandWeightForce() => handWeight * GRAVITY;

    // Torques
    public float GetElbowTorque() => totalLoadTorque;
    public float GetTorqueFromResistance() => torqueFromResistance;
    public float GetTorqueFromForearmWeight() => torqueFromForearmWeight;

    // Moment arms
    public float GetHandMomentArm() => handMomentArm;
    public float GetForearmMomentArm() => forearmMomentArm;
    public float GetMuscleMomentArm() => muscleMomentArm;

    // Muscle-specific
    public Vector3 GetMuscleForceDirection() => muscleForceDirection;
    public Vector3 GetMuscleInsertionPoint() => muscleInsertionPoint;
    public MuscleMode GetCurrentMuscleMode() => currentMuscleMode;

    // NEW: Resistance direction (DOWN for biceps/gravity, UP for triceps/cable)
    public Vector3 GetResistanceForceDirection() => resistanceForceDirection;

    // Tracking state
    public bool AllTracked() => shoulderTracked && elbowTracked && handTracked;
}