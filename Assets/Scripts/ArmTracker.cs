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
    public float handWeight = 0f; // Weight in kg

    [Header("Arm Properties")]
    public float armMass = 0.10f; // PASCO model forearm mass (CORRECTED from 2.0 kg)
    public float bicepInsertionDistance = 0.05f; // 5cm from elbow

    [Header("Smoothing")]
    public float smoothingFactor = 0.3f;

    // Raw positions from tracking
    private Vector3 shoulderPos;
    private Vector3 elbowPos;
    private Vector3 handPos;

    // Smoothed positions (for visualization)
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
    private float torqueAtElbow;
    private float requiredMuscleForce;
    private float handMomentArm;    // NEW: Store for getters
    private float forearmMomentArm; // NEW: Store for getters

    private const float GRAVITY = 9.81f; // m/s²

    void Start()
    {
        // Subscribe to tracking events
        shoulderTarget.OnTargetStatusChanged += OnShoulderStatusChanged;
        elbowTarget.OnTargetStatusChanged += OnElbowStatusChanged;
        handTarget.OnTargetStatusChanged += OnHandStatusChanged;

        // Initialize smoothed positions
        smoothedShoulderPos = Vector3.zero;
        smoothedElbowPos = Vector3.zero;
        smoothedHandPos = Vector3.zero;
    }

    void Update()
    {
        // Update positions if tracked (with smoothing)
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

        // Calculate physics (if all points are tracked)
        if (shoulderTracked && elbowTracked && handTracked)
        {
            CalculateArmPhysics();
        }

        UpdateDebugText();
    }

    void CalculateArmPhysics()
    {
        // Use smoothed positions for calculations
        upperArmLength = Vector3.Distance(smoothedShoulderPos, smoothedElbowPos);
        forearmLength = Vector3.Distance(smoothedElbowPos, smoothedHandPos);

        // Calculate elbow angle
        Vector3 upperArmDir = (smoothedElbowPos - smoothedShoulderPos).normalized;
        Vector3 forearmDir = (smoothedHandPos - smoothedElbowPos).normalized;
        elbowAngle = Vector3.Angle(upperArmDir, forearmDir);

        // Calculate forces and torques
        CalculateForces();
    }

    void CalculateForces()
    {
        // Weight forces
        float handWeightForce = handWeight * GRAVITY; // in Newtons
        float forearmWeightForce = armMass * GRAVITY;

        // ═══════════════════════════════════════════════════════════════════
        // CRITICAL FIX: Calculate TRUE moment arms (perpendicular distance)
        // For vertical forces (gravity), moment arm = horizontal distance
        // ═══════════════════════════════════════════════════════════════════

        // Hand moment arm: horizontal distance from elbow to hand
        Vector3 handProjection = new Vector3(
            smoothedHandPos.x,
            smoothedElbowPos.y,
            smoothedHandPos.z
        );
        handMomentArm = Vector3.Distance(smoothedElbowPos, handProjection);

        // Forearm center of mass moment arm
        Vector3 forearmCenter = (smoothedElbowPos + smoothedHandPos) / 2f;
        Vector3 forearmProjection = new Vector3(
            forearmCenter.x,
            smoothedElbowPos.y,
            forearmCenter.z
        );
        forearmMomentArm = Vector3.Distance(smoothedElbowPos, forearmProjection);

        // Torque = Force × Perpendicular Distance (moment arm)
        float handTorque = handWeightForce * handMomentArm;
        float forearmTorque = forearmWeightForce * forearmMomentArm;

        torqueAtElbow = handTorque + forearmTorque;

        // Calculate required muscle force (bicep)
        // Bicep has very short moment arm = mechanical disadvantage
        requiredMuscleForce = torqueAtElbow / bicepInsertionDistance;

        Debug.Log($"Elbow Angle: {elbowAngle:F1}°");
        Debug.Log($"Hand Moment Arm: {handMomentArm:F3} m");
        Debug.Log($"Torque at Elbow: {torqueAtElbow:F2} N⋅m");
        Debug.Log($"Required Muscle Force: {requiredMuscleForce:F2} N");
        Debug.Log($"Mechanical Advantage: {GetMechanicalAdvantage():F3}");
    }

    public void OnWeightUpdated()
    {
        // Called when user clicks "Update Forces" button
        if (float.TryParse(weightInput.text, out float weight))
        {
            handWeight = weight;
            Debug.Log($"Weight updated to: {handWeight} kg");
        }
        else
        {
            Debug.LogWarning("Invalid weight input!");
        }
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
                float ma = GetMechanicalAdvantage();
                physicsInfo = $"Elbow Angle: {elbowAngle:F1}°\n" +
                             $"Weight: {handWeight:F1} kg\n" +
                             $"Lever Arm: {handMomentArm:F2} m\n" +  // NEW
                             $"Torque: {torqueAtElbow:F2} N⋅m\n" +
                             $"Muscle Force: {requiredMuscleForce:F1} N\n" +
                             $"Mech. Adv.: {ma:F2}";  // NEW
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
        // Unsubscribe from events
        if (shoulderTarget != null)
            shoulderTarget.OnTargetStatusChanged -= OnShoulderStatusChanged;
        if (elbowTarget != null)
            elbowTarget.OnTargetStatusChanged -= OnElbowStatusChanged;
        if (handTarget != null)
            handTarget.OnTargetStatusChanged -= OnHandStatusChanged;
    }

    // ═══════════════════════════════════════════════════════════════════
    // PUBLIC GETTERS (for visualization and educational displays)
    // ═══════════════════════════════════════════════════════════════════
    public Vector3 GetShoulderPos() => smoothedShoulderPos;
    public Vector3 GetElbowPos() => smoothedElbowPos;
    public Vector3 GetHandPos() => smoothedHandPos;
    public float GetHandWeightForce() => handWeight * GRAVITY;
    public float GetForearmWeightForce() => armMass * GRAVITY;
    public float GetMuscleForce() => requiredMuscleForce;
    public float GetElbowTorque() => torqueAtElbow;
    public bool AllTracked() => shoulderTracked && elbowTracked && handTracked;

    // NEW GETTERS for educational displays
    public float GetHandMomentArm() => handMomentArm;
    public float GetForearmMomentArm() => forearmMomentArm;
    public float GetForearmLength() => forearmLength;
    public float GetBicepInsertionDistance() => bicepInsertionDistance;

    public float GetMechanicalAdvantage()
    {
        if (handMomentArm <= 0) return 0;
        return bicepInsertionDistance / handMomentArm;
    }

    // NEW: Get bicep insertion point for visualization
    public Vector3 GetBicepInsertionPoint()
    {
        if (!AllTracked()) return smoothedElbowPos;

        Vector3 forearmDir = (smoothedHandPos - smoothedElbowPos).normalized;
        return smoothedElbowPos + forearmDir * bicepInsertionDistance;
    }
}