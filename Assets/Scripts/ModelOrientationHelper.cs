using UnityEngine;

/// <summary>
/// HELPER: Attach this to your 3D model to auto-detect its orientation.
/// Run the scene, check the Console for recommended settings.
/// DELETE THIS AFTER SETUP.
/// </summary>
[ExecuteInEditMode]
public class ModelOrientationHelper : MonoBehaviour
{
    [Header("Click this to analyze the model")]
    public bool analyzeNow = false;

    [Header("═══ DETECTED SETTINGS ═══")]
    [Tooltip("Copy these to AutoAlignSkinController")]
    public string recommendedLengthAxis = "Not analyzed";
    public string recommendedTopDirection = "Not analyzed";
    public Vector3 detectedSize = Vector3.zero;
    public float detectedLength = 0f;

    [Header("═══ DEBUG VISUALIZATION ═══")]
    public bool showBounds = true;
    public bool showAxes = true;

    private Bounds localBounds;
    private bool hasAnalyzed = false;

    void Update()
    {
        if (analyzeNow)
        {
            analyzeNow = false;
            AnalyzeModel();
        }
    }

    void AnalyzeModel()
    {
        // Get bounds
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) mf = GetComponentInChildren<MeshFilter>();

        if (mf != null && mf.sharedMesh != null)
        {
            localBounds = mf.sharedMesh.bounds;
        }
        else
        {
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            if (smr == null) smr = GetComponentInChildren<SkinnedMeshRenderer>();

            if (smr != null && smr.sharedMesh != null)
            {
                localBounds = smr.sharedMesh.bounds;
            }
            else
            {
                Debug.LogError("No mesh found on this object!");
                return;
            }
        }

        // Account for scale
        Vector3 scaledSize = Vector3.Scale(localBounds.size, transform.localScale);
        detectedSize = scaledSize;

        // Find the longest axis
        int longestAxis = 0;
        float maxSize = scaledSize.x;

        if (scaledSize.y > maxSize)
        {
            longestAxis = 1;
            maxSize = scaledSize.y;
        }
        if (scaledSize.z > maxSize)
        {
            longestAxis = 2;
            maxSize = scaledSize.z;
        }

        detectedLength = maxSize;

        // Convert to axis name
        string[] axisNames = { "X", "Y", "Z" };
        recommendedLengthAxis = axisNames[longestAxis];

        // Determine if top is positive or negative
        // Assume the model is oriented "correctly" in the scene (top is up)
        // Check which end of the bounding box is higher in world space
        Vector3 positiveEnd = localBounds.center;
        Vector3 negativeEnd = localBounds.center;
        positiveEnd[longestAxis] = localBounds.max[longestAxis];
        negativeEnd[longestAxis] = localBounds.min[longestAxis];

        Vector3 positiveWorld = transform.TransformPoint(positiveEnd);
        Vector3 negativeWorld = transform.TransformPoint(negativeEnd);

        // If positive end is higher in world Y, then top is positive
        bool topIsPositive = positiveWorld.y > negativeWorld.y;
        recommendedTopDirection = topIsPositive ? "Positive (check the box)" : "Negative (uncheck the box)";

        hasAnalyzed = true;

        // Log results
        Debug.Log("═══════════════════════════════════════════════════════════════");
        Debug.Log($"MODEL ANALYSIS: {gameObject.name}");
        Debug.Log("═══════════════════════════════════════════════════════════════");
        Debug.Log($"  Bounding Box Size: X={scaledSize.x:F4}, Y={scaledSize.y:F4}, Z={scaledSize.z:F4}");
        Debug.Log($"  Longest Axis (Length): {recommendedLengthAxis} ({maxSize:F4} units)");
        Debug.Log($"  Top Direction: {recommendedTopDirection}");
        Debug.Log("");
        Debug.Log("COPY THESE TO AutoAlignSkinController:");
        Debug.Log($"  Length Axis = {recommendedLengthAxis}");
        Debug.Log($"  Top Is Positive = {topIsPositive}");
        Debug.Log("═══════════════════════════════════════════════════════════════");
    }

    void OnDrawGizmos()
    {
        if (!hasAnalyzed) return;

        if (showBounds)
        {
            // Draw bounding box
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(localBounds.center, localBounds.size);
        }

        if (showAxes)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Vector3 center = transform.TransformPoint(localBounds.center);
            float axisLength = detectedLength * 0.6f;

            // Draw local axes
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, center + transform.right * axisLength);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + transform.up * axisLength);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center, center + transform.forward * axisLength);

            // Mark the detected top and bottom
            Vector3 topLocal = localBounds.center;
            Vector3 bottomLocal = localBounds.center;
            int axis = recommendedLengthAxis == "X" ? 0 : (recommendedLengthAxis == "Y" ? 1 : 2);
            topLocal[axis] = localBounds.max[axis];
            bottomLocal[axis] = localBounds.min[axis];

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.TransformPoint(topLocal), detectedLength * 0.05f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.TransformPoint(bottomLocal), detectedLength * 0.05f);
        }
    }
}