using UnityEngine;
using TMPro;

public class MomentArmVisualizer : MonoBehaviour
{
    public ArmTracker armTracker;

    [Header("Visualization Settings")]
    private bool showMomentArms = true;
    public Color weightMomentArmColor = new Color(0, 1, 1, 0.7f); // Cyan for weight moment arms
    public Color muscleMomentArmColor = new Color(0, 1, 0, 0.7f); // Green for muscle moment arm

    // Weight moment arms
    private LineRenderer handMomentArmLine;
    private LineRenderer forearmMomentArmLine;
    private TextMeshPro handMomentLabel;
    private TextMeshPro forearmMomentLabel;

    // Muscle moment arm
    private LineRenderer muscleMomentArmLine;
    private TextMeshPro muscleMomentLabel;

    // Right angle markers
    private LineRenderer handRightAngle;
    private LineRenderer forearmRightAngle;
    private LineRenderer muscleRightAngle;

    void Start()
    {
        CreateMomentArmLines();
        CreateLabels();
        CreateRightAngleMarkers();
    }

    void CreateMomentArmLines()
    {
        // Hand moment arm line
        GameObject handLine = new GameObject("HandMomentArm");
        handLine.transform.SetParent(transform);
        handMomentArmLine = handLine.AddComponent<LineRenderer>();
        SetupDashedLine(handMomentArmLine, weightMomentArmColor);

        // Forearm moment arm line
        GameObject forearmLine = new GameObject("ForearmMomentArm");
        forearmLine.transform.SetParent(transform);
        forearmMomentArmLine = forearmLine.AddComponent<LineRenderer>();
        SetupDashedLine(forearmMomentArmLine, weightMomentArmColor);

        // Muscle moment arm line
        GameObject muscleLine = new GameObject("MuscleMomentArm");
        muscleLine.transform.SetParent(transform);
        muscleMomentArmLine = muscleLine.AddComponent<LineRenderer>();
        SetupDashedLine(muscleMomentArmLine, muscleMomentArmColor);
    }

    void SetupDashedLine(LineRenderer line, Color color)
    {
        line.startWidth = 0.004f;
        line.endWidth = 0.004f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.positionCount = 2;
        line.useWorldSpace = true;
    }

    void CreateLabels()
    {
        handMomentLabel = CreateLabel("r_hand", weightMomentArmColor);
        forearmMomentLabel = CreateLabel("r_arm", weightMomentArmColor);
        muscleMomentLabel = CreateLabel("r_muscle", muscleMomentArmColor);
    }

    TextMeshPro CreateLabel(string name, Color color)
    {
        GameObject labelObj = new GameObject($"Label_{name}");
        labelObj.transform.SetParent(transform);

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.fontSize = 0.25f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;

        labelObj.AddComponent<Billboard>();

        return tmp;
    }

    void CreateRightAngleMarkers()
    {
        // Right angle markers to show perpendicularity
        handRightAngle = CreateRightAngleMarker("HandRightAngle", weightMomentArmColor);
        forearmRightAngle = CreateRightAngleMarker("ForearmRightAngle", weightMomentArmColor);
        muscleRightAngle = CreateRightAngleMarker("MuscleRightAngle", muscleMomentArmColor);
    }

    LineRenderer CreateRightAngleMarker(string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.startWidth = 0.002f;
        line.endWidth = 0.002f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.positionCount = 3; // L-shape for right angle
        line.useWorldSpace = true;
        return line;
    }

    void Update()
    {
        if (showMomentArms && armTracker != null && armTracker.AllTracked())
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
        Vector3 muscleInsertionPoint = armTracker.GetMuscleInsertionPoint();
        Vector3 muscleDirection = armTracker.GetMuscleForceDirection();

        // ============================================================
        // 1. HAND MOMENT ARM (perpendicular distance to gravity line)
        // ============================================================
        // Gravity acts straight down, so moment arm is horizontal distance
        Vector3 handHorizontalProjection = new Vector3(handPos.x, elbowPos.y, handPos.z);

        handMomentArmLine.SetPosition(0, elbowPos);
        handMomentArmLine.SetPosition(1, handHorizontalProjection);
        handMomentArmLine.enabled = true;

        float handMomentArmLength = armTracker.GetHandMomentArm();
        Vector3 handLabelPos = (elbowPos + handHorizontalProjection) / 2f + Vector3.up * 0.02f;
        handMomentLabel.transform.position = handLabelPos;
        handMomentLabel.text = $"r⊥ = {handMomentArmLength * 100:F1} cm";
        handMomentLabel.gameObject.SetActive(true);

        // Right angle marker at projection point
        DrawRightAngle(handRightAngle, handHorizontalProjection,
            (elbowPos - handHorizontalProjection).normalized, Vector3.down, 0.015f);

        // ============================================================
        // 2. FOREARM MOMENT ARM
        // ============================================================
        Vector3 forearmHorizontalProjection = new Vector3(forearmCenter.x, elbowPos.y, forearmCenter.z);

        forearmMomentArmLine.SetPosition(0, elbowPos);
        forearmMomentArmLine.SetPosition(1, forearmHorizontalProjection);
        forearmMomentArmLine.enabled = true;

        float forearmMomentArmLength = armTracker.GetForearmMomentArm();
        Vector3 forearmLabelPos = (elbowPos + forearmHorizontalProjection) / 2f + Vector3.down * 0.02f;
        forearmMomentLabel.transform.position = forearmLabelPos;
        forearmMomentLabel.text = $"r⊥ = {forearmMomentArmLength * 100:F1} cm";
        forearmMomentLabel.gameObject.SetActive(true);

        // Right angle marker
        DrawRightAngle(forearmRightAngle, forearmHorizontalProjection,
            (elbowPos - forearmHorizontalProjection).normalized, Vector3.down, 0.015f);

        // ============================================================
        // 3. MUSCLE MOMENT ARM
        // ============================================================
        // The muscle moment arm is perpendicular distance from elbow to muscle force line
        // We visualize this by drawing from elbow perpendicular to the muscle force line

        Vector3 r_muscle = muscleInsertionPoint - elbowPos;

        // Project r_muscle onto the muscle direction to find the closest point on the force line
        float projLength = Vector3.Dot(r_muscle, muscleDirection);
        Vector3 projectionPoint = elbowPos + muscleDirection * projLength;

        // The moment arm goes from elbow perpendicular to the force line
        // But for clarity, we draw from elbow to the insertion point
        muscleMomentArmLine.SetPosition(0, elbowPos);
        muscleMomentArmLine.SetPosition(1, muscleInsertionPoint);
        muscleMomentArmLine.enabled = true;

        float muscleMomentArmLength = armTracker.GetMuscleMomentArm();
        Vector3 muscleLabelPos = (elbowPos + muscleInsertionPoint) / 2f +
            Vector3.Cross(muscleDirection, Vector3.up).normalized * 0.03f;
        muscleMomentLabel.transform.position = muscleLabelPos;
        muscleMomentLabel.text = $"r⊥ = {muscleMomentArmLength * 100:F1} cm";
        muscleMomentLabel.gameObject.SetActive(true);

        // Right angle marker (approximate)
        Vector3 perpDir = Vector3.Cross(muscleDirection, Vector3.up).normalized;
        if (perpDir.magnitude < 0.1f) perpDir = Vector3.Cross(muscleDirection, Vector3.forward).normalized;
        DrawRightAngle(muscleRightAngle, muscleInsertionPoint,
            -muscleDirection, perpDir, 0.01f);
    }

    void DrawRightAngle(LineRenderer line, Vector3 corner, Vector3 dir1, Vector3 dir2, float size)
    {
        if (line == null) return;

        Vector3 p1 = corner + dir1.normalized * size;
        Vector3 p2 = corner + dir1.normalized * size + dir2.normalized * size;
        Vector3 p3 = corner + dir2.normalized * size;

        line.SetPosition(0, p1);
        line.SetPosition(1, p2);
        line.SetPosition(2, p3);
        line.enabled = true;
    }

    void HideMomentArms()
    {
        if (handMomentArmLine != null) handMomentArmLine.enabled = false;
        if (forearmMomentArmLine != null) forearmMomentArmLine.enabled = false;
        if (muscleMomentArmLine != null) muscleMomentArmLine.enabled = false;

        if (handMomentLabel != null) handMomentLabel.gameObject.SetActive(false);
        if (forearmMomentLabel != null) forearmMomentLabel.gameObject.SetActive(false);
        if (muscleMomentLabel != null) muscleMomentLabel.gameObject.SetActive(false);

        if (handRightAngle != null) handRightAngle.enabled = false;
        if (forearmRightAngle != null) forearmRightAngle.enabled = false;
        if (muscleRightAngle != null) muscleRightAngle.enabled = false;
    }

    // PUBLIC METHOD FOR TOGGLE
    public void ToggleMomentArms(bool isOn)
    {
        showMomentArms = isOn;
        Debug.Log($"Moment Arms toggled: {isOn}");
    }
}