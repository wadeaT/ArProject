using UnityEngine;

/// <summary>
/// IMPROVED TripoArmController - Now supports SEPARATE scales for upper arm and forearm!
/// 
/// Changes from original:
/// - Added upperArmScaleMultiplier and forearmScaleMultiplier
/// - globalScaleMultiplier now acts as a base that individual scales multiply
/// </summary>
public class TripoArmController : MonoBehaviour
{
    [Header("═══ TRACKING ═══")]
    public ArmTracker armTracker;

    [Header("═══ MODELS ═══")]
    public Transform upperArmModel;
    public Transform forearmModel;

    [Tooltip("Check this if you have ONE model for the entire arm")]
    public bool useSingleArmModel = false;

    [Header("═══ MODEL PIVOT LOCATION ═══")]
    [Tooltip("Where is the model's pivot/origin point?")]
    public PivotLocation upperArmPivot = PivotLocation.Center;
    public PivotLocation forearmPivot = PivotLocation.Center;

    public enum PivotLocation
    {
        Center,
        TopEnd,
        BottomEnd
    }

    [Header("═══ UPPER ARM ALIGNMENT ═══")]
    public ModelEnd upperArmTopEnd = ModelEnd.PositiveY;
    public Vector3 upperArmRotationFix = Vector3.zero;

    [Tooltip("Position offset as PROPORTION of segment length (0.1 = 10% offset)")]
    public Vector3 upperArmProportionalOffset = Vector3.zero;

    [Tooltip("Fixed offset in meters (use sparingly - only for fine-tuning)")]
    public Vector3 upperArmFixedOffset = Vector3.zero;

    [Header("═══ FOREARM ALIGNMENT ═══")]
    public ModelEnd forearmTopEnd = ModelEnd.PositiveY;
    public Vector3 forearmRotationFix = Vector3.zero;

    [Tooltip("Position offset as PROPORTION of segment length (0.1 = 10% offset)")]
    public Vector3 forearmProportionalOffset = Vector3.zero;

    [Tooltip("Fixed offset in meters (use sparingly - only for fine-tuning)")]
    public Vector3 forearmFixedOffset = Vector3.zero;

    [Header("═══ SCALE ═══")]
    [Tooltip("Base scale multiplier for both models")]
    [Range(0.1f, 200f)]
    public float globalScaleMultiplier = 1f;

    [Tooltip("Additional scale multiplier for UPPER ARM only")]
    [Range(0.1f, 10f)]
    public float upperArmScaleMultiplier = 1f;

    [Tooltip("Additional scale multiplier for FOREARM only")]
    [Range(0.1f, 10f)]
    public float forearmScaleMultiplier = 1f;

    [Tooltip("Thickness relative to length (1.0 = natural proportions)")]
    [Range(0.1f, 5f)]
    public float thicknessRatio = 1f;

    [Tooltip("Additional thickness multiplier")]
    [Range(0.1f, 10f)]
    public float thicknessMultiplier = 1f;

    [Tooltip("Auto-scale thickness based on arm length")]
    public bool autoScaleThickness = true;

    [Tooltip("Reference arm length for thickness scaling (meters)")]
    public float referenceArmLength = 0.3f;

    [Range(-0.5f, 0.5f)]
    [Tooltip("Length adjustment as proportion (-0.1 = 10% shorter, 0.1 = 10% longer)")]
    public float lengthAdjustmentProportion = 0.0f;

    [Header("═══ APPEARANCE ═══")]
    public bool showModels = true;

    [Range(0.1f, 1f)]
    public float opacity = 0.75f;

    [Header("═══ SMOOTHING ═══")]
    [Range(0.01f, 0.3f)]
    public float smoothing = 0.1f;

    [Header("═══ DEBUG ═══")]
    public bool showDebugInfo = false;

    public enum ModelEnd
    {
        PositiveY, NegativeY,
        PositiveZ, NegativeZ,
        PositiveX, NegativeX
    }

    // Cached data
    private Vector3 upperArmOriginalScale;
    private Vector3 forearmOriginalScale;
    private Material upperArmMaterial;
    private Material forearmMaterial;

    // Smoothed transforms
    private Vector3 smoothedUpperPos;
    private Vector3 smoothedForearmPos;
    private Quaternion smoothedUpperRot = Quaternion.identity;
    private Quaternion smoothedForearmRot = Quaternion.identity;

    // Detected model info
    private int upperArmLengthAxis = 1;
    private int forearmLengthAxis = 1;
    private float upperArmModelLength;
    private float forearmModelLength;

    // Current segment lengths
    private float currentUpperArmLength;
    private float currentForearmLength;

    void Start()
    {
        AnalyzeModels();
        SetupMaterials();
    }

    void AnalyzeModels()
    {
        if (upperArmModel != null)
        {
            upperArmOriginalScale = upperArmModel.localScale;
            Bounds bounds = GetModelBounds(upperArmModel);
            Vector3 size = bounds.size;
            size.x *= upperArmOriginalScale.x;
            size.y *= upperArmOriginalScale.y;
            size.z *= upperArmOriginalScale.z;

            if (size.x >= size.y && size.x >= size.z)
                upperArmLengthAxis = 0;
            else if (size.y >= size.x && size.y >= size.z)
                upperArmLengthAxis = 1;
            else
                upperArmLengthAxis = 2;

            upperArmModelLength = size[upperArmLengthAxis];

            if (showDebugInfo)
                Debug.Log($"Upper arm model: length axis={upperArmLengthAxis}, length={upperArmModelLength:F4}");
        }

        if (forearmModel != null)
        {
            forearmOriginalScale = forearmModel.localScale;
            Bounds bounds = GetModelBounds(forearmModel);
            Vector3 size = bounds.size;
            size.x *= forearmOriginalScale.x;
            size.y *= forearmOriginalScale.y;
            size.z *= forearmOriginalScale.z;

            if (size.x >= size.y && size.x >= size.z)
                forearmLengthAxis = 0;
            else if (size.y >= size.x && size.y >= size.z)
                forearmLengthAxis = 1;
            else
                forearmLengthAxis = 2;

            forearmModelLength = size[forearmLengthAxis];

            if (showDebugInfo)
                Debug.Log($"Forearm model: length axis={forearmLengthAxis}, length={forearmModelLength:F4}");
        }
    }

    Bounds GetModelBounds(Transform model)
    {
        MeshFilter mf = model.GetComponent<MeshFilter>();
        if (mf == null) mf = model.GetComponentInChildren<MeshFilter>();

        if (mf != null && mf.sharedMesh != null)
            return mf.sharedMesh.bounds;

        Renderer rend = model.GetComponent<Renderer>();
        if (rend == null) rend = model.GetComponentInChildren<Renderer>();
        if (rend != null)
            return new Bounds(Vector3.zero, rend.bounds.size);

        return new Bounds(Vector3.zero, Vector3.one * 0.1f);
    }

    void SetupMaterials()
    {
        upperArmMaterial = GetMaterial(upperArmModel);
        forearmMaterial = GetMaterial(forearmModel);
        ApplyOpacity();
    }

    Material GetMaterial(Transform model)
    {
        if (model == null) return null;
        Renderer rend = model.GetComponent<Renderer>();
        if (rend == null) rend = model.GetComponentInChildren<Renderer>();
        if (rend == null) return null;

        rend.material = new Material(rend.material);
        return rend.material;
    }

    void ApplyOpacity()
    {
        if (upperArmMaterial != null) MakeTransparent(upperArmMaterial, opacity);
        if (forearmMaterial != null) MakeTransparent(forearmMaterial, opacity);
    }

    void MakeTransparent(Material mat, float alpha)
    {
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;

        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
    }

    void Update()
    {
        if (!showModels || armTracker == null || !armTracker.AllTracked())
        {
            SetVisibility(false);
            return;
        }

        SetVisibility(true);

        if (useSingleArmModel)
        {
            PositionSingleArmModel();
        }
        else
        {
            PositionUpperArm();
            PositionForearm();
        }
    }

    void PositionSingleArmModel()
    {
        if (upperArmModel == null) return;

        Vector3 shoulder = armTracker.GetShoulderPos();
        Vector3 hand = armTracker.GetHandPos();

        currentUpperArmLength = Vector3.Distance(shoulder, hand);

        StretchModelBetweenPoints(
            upperArmModel,
            shoulder,
            hand,
            upperArmLengthAxis,
            upperArmOriginalScale,
            upperArmModelLength,
            upperArmTopEnd,
            upperArmPivot,
            upperArmRotationFix,
            upperArmProportionalOffset,
            upperArmFixedOffset,
            currentUpperArmLength,
            globalScaleMultiplier * upperArmScaleMultiplier, // Combined scale
            ref smoothedUpperPos,
            ref smoothedUpperRot
        );
    }

    void PositionUpperArm()
    {
        if (upperArmModel == null) return;

        Vector3 shoulder = armTracker.GetShoulderPos();
        Vector3 elbow = armTracker.GetElbowPos();

        currentUpperArmLength = Vector3.Distance(shoulder, elbow);

        StretchModelBetweenPoints(
            upperArmModel,
            shoulder,
            elbow,
            upperArmLengthAxis,
            upperArmOriginalScale,
            upperArmModelLength,
            upperArmTopEnd,
            upperArmPivot,
            upperArmRotationFix,
            upperArmProportionalOffset,
            upperArmFixedOffset,
            currentUpperArmLength,
            globalScaleMultiplier * upperArmScaleMultiplier, // Combined scale
            ref smoothedUpperPos,
            ref smoothedUpperRot
        );
    }

    void PositionForearm()
    {
        if (forearmModel == null) return;

        Vector3 elbow = armTracker.GetElbowPos();
        Vector3 hand = armTracker.GetHandPos();

        currentForearmLength = Vector3.Distance(elbow, hand);

        StretchModelBetweenPoints(
            forearmModel,
            elbow,
            hand,
            forearmLengthAxis,
            forearmOriginalScale,
            forearmModelLength,
            forearmTopEnd,
            forearmPivot,
            forearmRotationFix,
            forearmProportionalOffset,
            forearmFixedOffset,
            currentForearmLength,
            globalScaleMultiplier * forearmScaleMultiplier, // Combined scale
            ref smoothedForearmPos,
            ref smoothedForearmRot
        );
    }

    void StretchModelBetweenPoints(
        Transform model,
        Vector3 startPoint,
        Vector3 endPoint,
        int lengthAxis,
        Vector3 originalScale,
        float modelLength,
        ModelEnd topEnd,
        PivotLocation pivotLocation,
        Vector3 rotationFix,
        Vector3 proportionalOffset,
        Vector3 fixedOffset,
        float segmentLength,
        float scaleMultiplier, // NEW: individual scale
        ref Vector3 smoothedPos,
        ref Quaternion smoothedRot)
    {
        Vector3 direction = (endPoint - startPoint).normalized;
        float targetLength = segmentLength * (1f + lengthAdjustmentProportion);

        // POSITION
        Vector3 basePosition;
        switch (pivotLocation)
        {
            case PivotLocation.TopEnd:
                basePosition = startPoint;
                break;
            case PivotLocation.BottomEnd:
                basePosition = endPoint;
                break;
            case PivotLocation.Center:
            default:
                basePosition = (startPoint + endPoint) / 2f;
                break;
        }

        Vector3 scaledProportionalOffset = new Vector3(
            proportionalOffset.x * segmentLength,
            proportionalOffset.y * segmentLength,
            proportionalOffset.z * segmentLength
        );

        Vector3 targetPos = basePosition + scaledProportionalOffset + fixedOffset;

        if (smoothedPos == Vector3.zero) smoothedPos = targetPos;
        smoothedPos = Vector3.Lerp(smoothedPos, targetPos, 1f - smoothing);

        // ROTATION
        Vector3 modelAxis = GetAxisVector(lengthAxis, IsPositiveEnd(topEnd));
        Quaternion alignRotation = Quaternion.FromToRotation(modelAxis, direction);
        Quaternion fixRotation = Quaternion.Euler(rotationFix);
        Quaternion targetRot = alignRotation * fixRotation;

        smoothedRot = Quaternion.Slerp(smoothedRot, targetRot, 1f - smoothing);

        model.rotation = smoothedRot;
        model.position = smoothedPos;

        // SCALE - using individual scaleMultiplier
        if (modelLength <= 0.001f) modelLength = 0.1f;
        float lengthScale = targetLength / modelLength;

        float effectiveThickness = thicknessMultiplier;

        if (autoScaleThickness && referenceArmLength > 0.01f)
        {
            float armSizeRatio = segmentLength / referenceArmLength;
            effectiveThickness *= armSizeRatio * thicknessRatio;
        }
        else
        {
            effectiveThickness *= thicknessRatio;
        }

        Vector3 newScale = originalScale * scaleMultiplier; // Use individual scale
        newScale[lengthAxis] *= lengthScale;

        for (int i = 0; i < 3; i++)
        {
            if (i != lengthAxis)
                newScale[i] *= effectiveThickness;
        }

        model.localScale = newScale;

        if (showDebugInfo)
        {
            Debug.Log($"Segment: {segmentLength:F3}m, Scale: {newScale}, ScaleMult: {scaleMultiplier:F2}");
        }
    }

    Vector3 GetAxisVector(int axis, bool positive)
    {
        int sign = positive ? 1 : -1;
        switch (axis)
        {
            case 0: return Vector3.right * sign;
            case 1: return Vector3.up * sign;
            case 2: return Vector3.forward * sign;
            default: return Vector3.up * sign;
        }
    }

    bool IsPositiveEnd(ModelEnd end)
    {
        return end == ModelEnd.PositiveX ||
               end == ModelEnd.PositiveY ||
               end == ModelEnd.PositiveZ;
    }

    void SetVisibility(bool visible)
    {
        if (upperArmModel != null)
            upperArmModel.gameObject.SetActive(visible);
        if (forearmModel != null && !useSingleArmModel)
            forearmModel.gameObject.SetActive(visible);
    }

    // ═══════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════

    public void ToggleSkin(bool isOn)
    {
        showModels = isOn;
        SetVisibility(isOn && armTracker != null && armTracker.AllTracked());
        Debug.Log($"Skin display: {(isOn ? "ON" : "OFF")}");
    }

    public float GetUpperArmLength() => currentUpperArmLength;
    public float GetForearmLength() => currentForearmLength;

    // Backward compatibility
    public Vector3 upperArmPositionOffset
    {
        get => upperArmProportionalOffset;
        set => upperArmProportionalOffset = value;
    }

    public Vector3 forearmPositionOffset
    {
        get => forearmProportionalOffset;
        set => forearmProportionalOffset = value;
    }
}