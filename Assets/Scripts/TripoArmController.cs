using UnityEngine;

/// <summary>
/// Controls Tripo AI arm models - positions and scales them to match tracked markers.
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

    [Header("═══ UPPER ARM ALIGNMENT ═══")]
    public ModelEnd upperArmTopEnd = ModelEnd.PositiveY;
    public Vector3 upperArmRotationFix = Vector3.zero;
    public Vector3 upperArmPositionOffset = Vector3.zero;

    [Header("═══ FOREARM ALIGNMENT ═══")]
    public ModelEnd forearmTopEnd = ModelEnd.PositiveY;
    public Vector3 forearmRotationFix = Vector3.zero;
    public Vector3 forearmPositionOffset = Vector3.zero;

    [Header("═══ SCALE ═══")]
    [Range(0.1f, 200f)]
    public float globalScaleMultiplier = 50f;

    [Range(0.1f, 10f)]
    public float thicknessMultiplier = 3f;

    [Range(-0.1f, 0.1f)]
    public float lengthAdjustment = 0.0f;

    [Header("═══ APPEARANCE ═══")]
    public bool showModels = true;

    [Range(0.1f, 1f)]
    public float opacity = 0.75f;

    [Header("═══ SMOOTHING ═══")]
    [Range(0.01f, 0.3f)]
    public float smoothing = 0.1f;

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

            // Find longest axis
            if (size.x >= size.y && size.x >= size.z)
                upperArmLengthAxis = 0;
            else if (size.y >= size.x && size.y >= size.z)
                upperArmLengthAxis = 1;
            else
                upperArmLengthAxis = 2;

            upperArmModelLength = size[upperArmLengthAxis];
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

        StretchModelBetweenPoints(
            upperArmModel,
            armTracker.GetShoulderPos(),
            armTracker.GetHandPos(),
            upperArmLengthAxis,
            upperArmOriginalScale,
            upperArmModelLength,
            upperArmTopEnd,
            upperArmRotationFix,
            upperArmPositionOffset,
            ref smoothedUpperPos,
            ref smoothedUpperRot
        );
    }

    void PositionUpperArm()
    {
        if (upperArmModel == null) return;

        StretchModelBetweenPoints(
            upperArmModel,
            armTracker.GetShoulderPos(),
            armTracker.GetElbowPos(),
            upperArmLengthAxis,
            upperArmOriginalScale,
            upperArmModelLength,
            upperArmTopEnd,
            upperArmRotationFix,
            upperArmPositionOffset,
            ref smoothedUpperPos,
            ref smoothedUpperRot
        );
    }

    void PositionForearm()
    {
        if (forearmModel == null) return;

        StretchModelBetweenPoints(
            forearmModel,
            armTracker.GetElbowPos(),
            armTracker.GetHandPos(),
            forearmLengthAxis,
            forearmOriginalScale,
            forearmModelLength,
            forearmTopEnd,
            forearmRotationFix,
            forearmPositionOffset,
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
        Vector3 rotationFix,
        Vector3 positionOffset,
        ref Vector3 smoothedPos,
        ref Quaternion smoothedRot)
    {
        Vector3 direction = (endPoint - startPoint).normalized;
        float targetLength = Vector3.Distance(startPoint, endPoint) + lengthAdjustment;
        Vector3 centerPos = (startPoint + endPoint) / 2f;

        // Smooth position
        if (smoothedPos == Vector3.zero) smoothedPos = centerPos;
        smoothedPos = Vector3.Lerp(smoothedPos, centerPos, 1f - smoothing);

        // Calculate rotation
        Vector3 modelAxis = GetAxisVector(lengthAxis, IsPositiveEnd(topEnd));
        Quaternion alignRotation = Quaternion.FromToRotation(modelAxis, direction);
        Quaternion fixRotation = Quaternion.Euler(rotationFix);
        Quaternion targetRot = alignRotation * fixRotation;

        smoothedRot = Quaternion.Slerp(smoothedRot, targetRot, 1f - smoothing);

        // Apply transforms
        model.rotation = smoothedRot;
        model.position = smoothedPos + positionOffset;

        // Calculate scale
        if (modelLength <= 0.001f) modelLength = 0.1f;
        float lengthScale = targetLength / modelLength;

        Vector3 newScale = originalScale * globalScaleMultiplier;
        newScale[lengthAxis] *= lengthScale;

        for (int i = 0; i < 3; i++)
        {
            if (i != lengthAxis)
                newScale[i] *= thicknessMultiplier;
        }

        model.localScale = newScale;
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
    // PUBLIC METHOD - Connect this to your UI Toggle!
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Toggle skin visibility - connect to UI Toggle's OnValueChanged
    /// </summary>
    public void ToggleSkin(bool isOn)
    {
        showModels = isOn;
        SetVisibility(isOn && armTracker != null && armTracker.AllTracked());
        Debug.Log($"Skin display: {(isOn ? "ON" : "OFF")}");
    }
}