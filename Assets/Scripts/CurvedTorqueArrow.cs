using UnityEngine;
using TMPro;

public class CurvedTorqueArrow : MonoBehaviour
{
    public ArmTracker armTracker;

    private bool showCurvedArrow = false;
    private LineRenderer curvedLine;
    private GameObject arrowTip;
    private TextMeshPro label;

    [Header("Settings")]
    public float radius = 0.08f;
    public int curveSegments = 20;
    public Color arrowColor = Color.yellow;

    void Start()
    {
        CreateCurvedArrow();
        CreateLabel();
    }

    void CreateCurvedArrow()
    {
        GameObject curveObj = new GameObject("CurvedTorqueArrow");
        curveObj.transform.SetParent(transform);
        curvedLine = curveObj.AddComponent<LineRenderer>();

        curvedLine.startWidth = 0.006f;
        curvedLine.endWidth = 0.006f;
        curvedLine.material = new Material(Shader.Find("Sprites/Default"));
        curvedLine.startColor = arrowColor;
        curvedLine.endColor = arrowColor;
        curvedLine.positionCount = curveSegments;
        curvedLine.useWorldSpace = true;

        // Arrow tip (small cube as arrow head)
        arrowTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrowTip.transform.SetParent(transform);
        arrowTip.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        arrowTip.GetComponent<Renderer>().material.color = arrowColor;
        Destroy(arrowTip.GetComponent<Collider>());
    }

    void CreateLabel()
    {
        GameObject labelObj = new GameObject("TorqueDirectionLabel");
        labelObj.transform.SetParent(transform);

        label = labelObj.AddComponent<TextMeshPro>();
        label.fontSize = 0.25f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = arrowColor;

        labelObj.AddComponent<Billboard>();
    }

    void Update()
    {
        if (showCurvedArrow && armTracker != null && armTracker.AllTracked())
        {
            DrawCurvedArrow();
        }
        else
        {
            HideCurvedArrow();
        }
    }

    void DrawCurvedArrow()
    {
        Vector3 elbowPos = armTracker.GetElbowPos();
        Vector3 handPos = armTracker.GetHandPos();

        // Calculate direction from elbow to hand (in XZ plane)
        Vector3 armDirection = handPos - elbowPos;
        armDirection.y = 0; // Flatten to horizontal plane

        if (armDirection.magnitude < 0.01f)
        {
            HideCurvedArrow();
            return;
        }

        armDirection.Normalize();

        // Draw curved arc around elbow (counter-clockwise when viewed from above)
        float startAngle = Mathf.Atan2(armDirection.z, armDirection.x);
        float arcLength = 120f * Mathf.Deg2Rad; // 120 degree arc

        for (int i = 0; i < curveSegments; i++)
        {
            float angle = startAngle + (arcLength * i / (curveSegments - 1));
            float x = elbowPos.x + Mathf.Cos(angle) * radius;
            float z = elbowPos.z + Mathf.Sin(angle) * radius;

            curvedLine.SetPosition(i, new Vector3(x, elbowPos.y + 0.05f, z));
        }

        curvedLine.enabled = true;

        // Position arrow tip at end of curve
        Vector3 lastPoint = curvedLine.GetPosition(curveSegments - 1);
        Vector3 secondToLast = curvedLine.GetPosition(curveSegments - 2);
        Vector3 tipDirection = (lastPoint - secondToLast).normalized;

        arrowTip.transform.position = lastPoint;
        arrowTip.transform.rotation = Quaternion.LookRotation(tipDirection);
        arrowTip.SetActive(true);

        // Label
        float torque = armTracker.GetElbowTorque();
        label.transform.position = elbowPos + Vector3.up * 0.12f;
        label.text = $"↻ Torque\n{torque:F1} N⋅m";
        label.gameObject.SetActive(true);
    }

    void HideCurvedArrow()
    {
        if (curvedLine != null) curvedLine.enabled = false;
        if (arrowTip != null) arrowTip.SetActive(false);
        if (label != null) label.gameObject.SetActive(false);
    }

    public void ToggleCurvedArrow(bool isOn)
    {
        showCurvedArrow = isOn;
        Debug.Log($"Curved Torque Arrow toggled: {isOn}");
    }
}