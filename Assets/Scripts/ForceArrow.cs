using UnityEngine;

public class ForceArrow : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private GameObject arrowHead;

    [Header("Arrow Settings")]
    public Color arrowColor = Color.red;
    public float forceScale = 0.002f; // Scale factor: 1N = 0.01m
    public float arrowWidth = 0.005f;
    public float arrowHeadSize = 0.02f;

    void Awake()
    {
        // Create LineRenderer for arrow shaft
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = arrowWidth;
        lineRenderer.endWidth = arrowWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = arrowColor;
        lineRenderer.endColor = arrowColor;
        lineRenderer.positionCount = 2;

        // Create arrow head using Cylinder (rotated and scaled to look like arrow tip)
        arrowHead = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arrowHead.transform.SetParent(transform);
        arrowHead.transform.localScale = new Vector3(arrowHeadSize * 2, arrowHeadSize, arrowHeadSize * 2);

        Renderer renderer = arrowHead.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = arrowColor;

        // Remove collider (not needed for visualization)
        Destroy(arrowHead.GetComponent<Collider>());
    }

    public void DrawArrow(Vector3 startPos, Vector3 direction, float forceMagnitude, float scaleFactor = 0.002f)
    {
        if (forceMagnitude <= 0)
        {
            SetVisibility(false);
            return;
        }

        // Calculate arrow length with scale factor
        float arrowLength = forceMagnitude * scaleFactor;

        // Clamp to reasonable limits (prevent arrows from being too long or too short)
        arrowLength = Mathf.Clamp(arrowLength, 0.02f, 0.5f);

        Vector3 endPos = startPos + direction.normalized * arrowLength;

        // Update line renderer
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        // Update arrow head position and rotation
        arrowHead.transform.position = endPos;
        arrowHead.transform.rotation = Quaternion.LookRotation(direction);
        arrowHead.transform.Rotate(90, 0, 0);

        SetVisibility(true);
    }

    public void SetColor(Color color)
    {
        arrowColor = color;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        arrowHead.GetComponent<Renderer>().material.color = color;
    }

    public void SetVisibility(bool visible)
    {
        lineRenderer.enabled = visible;
        arrowHead.SetActive(visible);
    }
}