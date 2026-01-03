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
    public Color loadTorqueColor = Color.red;      // Color for load torque (weight)
    public Color muscleTorqueColor = Color.green;  // Color for muscle torque

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
        curvedLine.startColor = loadTorqueColor;
        curvedLine.endColor = loadTorqueColor;
        curvedLine.positionCount = curveSegments;
        curvedLine.useWorldSpace = true;

        // Arrow tip (small cone-like shape)
        arrowTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrowTip.transform.SetParent(transform);
        arrowTip.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        arrowTip.GetComponent<Renderer>().material.color = loadTorqueColor;
        Destroy(arrowTip.GetComponent<Collider>());
    }

    void CreateLabel()
    {
        GameObject labelObj = new GameObject("TorqueDirectionLabel");
        labelObj.transform.SetParent(transform);

        label = labelObj.AddComponent<TextMeshPro>();
        label.fontSize = 0.25f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = loadTorqueColor;

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
        Vector3 shoulderPos = armTracker.GetShoulderPos();

        // ============================================================
        // ROTATION DIRECTION
        // ============================================================
        // The weight creates a torque that would rotate the forearm
        // in the direction of "opening" the elbow (extension)
        // 
        // The muscle creates an opposing torque (flexion for biceps)
        // 
        // We visualize the direction the arm would rotate under 
        // the influence of gravity (the load torque)
        // ============================================================

        // Calculate the plane of rotation
        // The rotation happens around an axis perpendicular to the arm plane
        Vector3 armDirection = (handPos - elbowPos).normalized;

        // Use the cross product to find the rotation axis
        Vector3 gravityForce = Vector3.down;
        Vector3 rotationAxis = Vector3.Cross(armDirection, gravityForce).normalized;

        if (rotationAxis.magnitude < 0.01f)
        {
            // Arm is vertical, use default axis
            rotationAxis = Vector3.forward;
        }

        // Calculate start angle in the rotation plane
        // We draw the arc in the plane perpendicular to the rotation axis
        Vector3 startDir = armDirection;

        // Arc parameters
        float arcAngleDegrees = 90f; // How much of the circle to draw
        bool clockwise = Vector3.Dot(rotationAxis, Vector3.Cross(armDirection, gravityForce)) > 0;

        // Draw the curved arc
        for (int i = 0; i < curveSegments; i++)
        {
            float t = (float)i / (curveSegments - 1);
            float angle = t * arcAngleDegrees * (clockwise ? 1 : -1);

            // Rotate around the rotation axis
            Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis);
            Vector3 point = elbowPos + rotation * (startDir * radius);

            // Offset slightly above the arm plane for visibility
            point += rotationAxis * 0.02f;

            curvedLine.SetPosition(i, point);
        }

        curvedLine.enabled = true;

        // Position arrow tip at end of curve
        Vector3 lastPoint = curvedLine.GetPosition(curveSegments - 1);
        Vector3 secondToLast = curvedLine.GetPosition(curveSegments - 2);
        Vector3 tipDirection = (lastPoint - secondToLast).normalized;

        arrowTip.transform.position = lastPoint;
        arrowTip.transform.rotation = Quaternion.LookRotation(tipDirection);
        arrowTip.SetActive(true);

        // Label showing torque value and balance equation
        float loadTorque = armTracker.GetElbowTorque();
        float muscleForce = armTracker.GetMuscleForce();

        label.transform.position = elbowPos + Vector3.up * 0.12f;
        label.text = $"↻ Load Torque\n" +
                    $"τ = {loadTorque:F1} N⋅m\n\n" +
                    $"⚖️ Balance:\n" +
                    $"τ_load + τ_muscle = 0";
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