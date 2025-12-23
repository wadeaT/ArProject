using UnityEngine;
using TMPro;

public class MomentArmVisualizer : MonoBehaviour
{
    public ArmTracker armTracker;

    [Header("Visualization Settings")]
    private bool showMomentArms = true;
    public Color momentArmColor = new Color(0, 1, 1, 0.7f); // Cyan, semi-transparent

    private LineRenderer handMomentArm;
    private LineRenderer forearmMomentArm;
    private TextMeshPro handMomentLabel;
    private TextMeshPro forearmMomentLabel;

    void Start()
    {
        CreateMomentArmLines();
        CreateLabels();
    }

    void CreateMomentArmLines()
    {
        // Hand moment arm line
        GameObject handLine = new GameObject("HandMomentArm");
        handLine.transform.SetParent(transform);
        handMomentArm = handLine.AddComponent<LineRenderer>();
        SetupDashedLine(handMomentArm);

        // Forearm moment arm line
        GameObject forearmLine = new GameObject("ForearmMomentArm");
        forearmLine.transform.SetParent(transform);
        forearmMomentArm = forearmLine.AddComponent<LineRenderer>();
        SetupDashedLine(forearmMomentArm);
    }

    void SetupDashedLine(LineRenderer line)
    {
        line.startWidth = 0.004f;
        line.endWidth = 0.004f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = momentArmColor;
        line.endColor = momentArmColor;
        line.positionCount = 2;
        line.useWorldSpace = true;
    }

    void CreateLabels()
    {
        handMomentLabel = CreateLabel("r_hand");
        forearmMomentLabel = CreateLabel("r_arm");
    }

    TextMeshPro CreateLabel(string name)
    {
        GameObject labelObj = new GameObject($"Label_{name}");
        labelObj.transform.SetParent(transform);

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.fontSize = 0.25f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.cyan;

        labelObj.AddComponent<Billboard>();

        return tmp;
    }

    void Update()
    {
        if (showMomentArms && armTracker.AllTracked())
        {
            DrawMomentArms();
        }
        else
        {
            HideMomentArms();
        }
    }

    void DrawMomentArms()
    {
        Vector3 elbowPos = armTracker.GetElbowPos();
        Vector3 handPos = armTracker.GetHandPos();
        Vector3 forearmCenter = (elbowPos + handPos) / 2f;

        // Create horizontal projection points (perpendicular distance)
        Vector3 handProjection = new Vector3(handPos.x, elbowPos.y, handPos.z);
        Vector3 forearmProjection = new Vector3(forearmCenter.x, elbowPos.y, forearmCenter.z);

        // Draw hand moment arm (dashed line from elbow to projection point)
        handMomentArm.SetPosition(0, elbowPos);
        handMomentArm.SetPosition(1, handProjection);
        handMomentArm.enabled = true;

        // Draw forearm moment arm
        forearmMomentArm.SetPosition(0, elbowPos);
        forearmMomentArm.SetPosition(1, forearmProjection);
        forearmMomentArm.enabled = true;

        // Calculate actual moment arm lengths
        float handMomentLength = Vector3.Distance(elbowPos, handProjection);
        float forearmMomentLength = Vector3.Distance(elbowPos, forearmProjection);

        // Update labels
        Vector3 handLabelPos = (elbowPos + handProjection) / 2f + Vector3.up * 0.03f;
        handMomentLabel.transform.position = handLabelPos;
        handMomentLabel.text = $"r⊥ = {handMomentLength:F2}m";

        Vector3 forearmLabelPos = (elbowPos + forearmProjection) / 2f + Vector3.down * 0.03f;
        forearmMomentLabel.transform.position = forearmLabelPos;
        forearmMomentLabel.text = $"r⊥ = {forearmMomentLength:F2}m";
    }

    void HideMomentArms()
    {
        if (handMomentArm != null) handMomentArm.enabled = false;
        if (forearmMomentArm != null) forearmMomentArm.enabled = false;
        if (handMomentLabel != null) handMomentLabel.gameObject.SetActive(false);
        if (forearmMomentLabel != null) forearmMomentLabel.gameObject.SetActive(false);
    }

    // PUBLIC METHOD FOR TOGGLE - This is what Unity needs!
    public void ToggleMomentArms(bool isOn)
    {
        showMomentArms = isOn;
        Debug.Log($"Moment Arms toggled: {isOn}");
    }
}