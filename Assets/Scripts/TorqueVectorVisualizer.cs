using UnityEngine;
using TMPro;

public class TorqueVectorVisualizer : MonoBehaviour
{
    public ArmTracker armTracker;

    [Header("Visualization Settings")]
    private bool showTorqueVector = false; // Start hidden (advanced concept)
    public Color torqueColor = new Color(1f, 0.84f, 0f); // Gold color
    public float torqueScale = 0.02f; // Scale factor for visualization

    private LineRenderer torqueArrow;
    private GameObject arrowHead;
    private TextMeshPro torqueLabel;

    void Start()
    {
        CreateTorqueArrow();
        CreateLabel();
    }

    void CreateTorqueArrow()
    {
        // Create arrow shaft
        GameObject shaftObj = new GameObject("TorqueArrowShaft");
        shaftObj.transform.SetParent(transform);
        torqueArrow = shaftObj.AddComponent<LineRenderer>();

        torqueArrow.startWidth = 0.006f;
        torqueArrow.endWidth = 0.006f;
        torqueArrow.material = new Material(Shader.Find("Sprites/Default"));
        torqueArrow.startColor = torqueColor;
        torqueArrow.endColor = torqueColor;
        torqueArrow.positionCount = 2;
        torqueArrow.useWorldSpace = true;

        // Create arrow head
        arrowHead = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arrowHead.transform.SetParent(transform);
        arrowHead.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);

        Renderer renderer = arrowHead.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = torqueColor;

        Destroy(arrowHead.GetComponent<Collider>());
    }

    void CreateLabel()
    {
        GameObject labelObj = new GameObject("TorqueLabel");
        labelObj.transform.SetParent(transform);

        torqueLabel = labelObj.AddComponent<TextMeshPro>();
        torqueLabel.fontSize = 0.3f;
        torqueLabel.alignment = TextAlignmentOptions.Center;
        torqueLabel.color = torqueColor;

        labelObj.AddComponent<Billboard>();
    }

    void Update()
    {
        if (showTorqueVector && armTracker != null && armTracker.AllTracked())
        {
            DrawTorqueVector();
        }
        else
        {
            HideTorqueVector();
        }
    }

    void DrawTorqueVector()
    {
        Vector3 elbowPos = armTracker.GetElbowPos();
        Vector3 shoulderPos = armTracker.GetShoulderPos();
        Vector3 handPos = armTracker.GetHandPos();

        // Calculate the rotation axis (perpendicular to the arm plane)
        // Using cross product to find perpendicular direction
        Vector3 upperArm = (elbowPos - shoulderPos).normalized;
        Vector3 forearm = (handPos - elbowPos).normalized;

        // Torque direction using right-hand rule
        // τ = r × F (cross product gives perpendicular vector)
        Vector3 torqueDirection = Vector3.Cross(forearm, Vector3.down).normalized;

        // If cross product gives zero (arm is vertical), use a default direction
        if (torqueDirection.magnitude < 0.01f)
        {
            torqueDirection = Vector3.forward;
        }

        // Get torque magnitude from physics calculations
        float torqueMagnitude = armTracker.GetElbowTorque();

        // Calculate arrow length based on torque magnitude
        float arrowLength = torqueMagnitude * torqueScale;
        arrowLength = Mathf.Clamp(arrowLength, 0.05f, 0.4f); // Clamp to reasonable size

        // Draw arrow from elbow along rotation axis
        Vector3 startPos = elbowPos;
        Vector3 endPos = startPos + torqueDirection * arrowLength;

        // Update line renderer
        torqueArrow.SetPosition(0, startPos);
        torqueArrow.SetPosition(1, endPos);
        torqueArrow.enabled = true;

        // Update arrow head
        arrowHead.transform.position = endPos;
        arrowHead.transform.rotation = Quaternion.LookRotation(torqueDirection);
        arrowHead.transform.Rotate(90, 0, 0);
        arrowHead.SetActive(true);

        // Update label
        torqueLabel.transform.position = endPos + torqueDirection * 0.05f;
        torqueLabel.text = $"TORQUE (τ)\n{torqueMagnitude:F1} N⋅m\n(rotation axis)";
        torqueLabel.gameObject.SetActive(true);
    }

    void HideTorqueVector()
    {
        if (torqueArrow != null) torqueArrow.enabled = false;
        if (arrowHead != null) arrowHead.SetActive(false);
        if (torqueLabel != null) torqueLabel.gameObject.SetActive(false);
    }

    // PUBLIC METHOD FOR TOGGLE
    public void ToggleTorqueVector(bool isOn)
    {
        showTorqueVector = isOn;
        Debug.Log($"Torque Vector toggled: {isOn}");
    }
}