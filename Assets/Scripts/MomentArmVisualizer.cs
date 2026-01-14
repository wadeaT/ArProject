using UnityEngine;

public class MomentArmVisualizer : MonoBehaviour
{
    public ArmTracker armTracker;

    [Header("Visualization Settings")]
    private bool showMomentArms = true;
    public Color weightMomentArmColor = new Color(0, 1, 1, 0.7f);
    public Color muscleMomentArmColor = new Color(0, 1, 0, 0.7f);

    // Moment arm lines (no labels)
    private LineRenderer handMomentArmLine;
    private LineRenderer forearmMomentArmLine;
    private LineRenderer muscleMomentArmLine;

    // Right angle markers
    private LineRenderer handRightAngle;
    private LineRenderer forearmRightAngle;
    private LineRenderer muscleRightAngle;

    // Cached values for external access
    private float cachedHandMomentArm;
    private float cachedForearmMomentArm;
    private float cachedMuscleMomentArm;

    void Start()
    {
        CreateMomentArmLines();
        CreateRightAngleMarkers();
    }

    void CreateMomentArmLines()
    {
        GameObject handLine = new GameObject("HandMomentArm");
        handLine.transform.SetParent(transform);
        handMomentArmLine = handLine.AddComponent<LineRenderer>();
        SetupDashedLine(handMomentArmLine, weightMomentArmColor);

        GameObject forearmLine = new GameObject("ForearmMomentArm");
        forearmLine.transform.SetParent(transform);
        forearmMomentArmLine = forearmLine.AddComponent<LineRenderer>();
        SetupDashedLine(forearmMomentArmLine, weightMomentArmColor);

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

    void CreateRightAngleMarkers()
    {
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
        line.positionCount = 3;
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

        // 1. HAND MOMENT ARM
        Vector3 handHorizontalProjection = new Vector3(handPos.x, elbowPos.y, handPos.z);

        handMomentArmLine.SetPosition(0, elbowPos);
        handMomentArmLine.SetPosition(1, handHorizontalProjection);
        handMomentArmLine.enabled = true;

        cachedHandMomentArm = armTracker.GetHandMomentArm();

        DrawRightAngle(handRightAngle, handHorizontalProjection,
            (elbowPos - handHorizontalProjection).normalized, Vector3.down, 0.015f);

        // 2. FOREARM MOMENT ARM
        Vector3 forearmHorizontalProjection = new Vector3(forearmCenter.x, elbowPos.y, forearmCenter.z);

        forearmMomentArmLine.SetPosition(0, elbowPos);
        forearmMomentArmLine.SetPosition(1, forearmHorizontalProjection);
        forearmMomentArmLine.enabled = true;

        cachedForearmMomentArm = armTracker.GetForearmMomentArm();

        DrawRightAngle(forearmRightAngle, forearmHorizontalProjection,
            (elbowPos - forearmHorizontalProjection).normalized, Vector3.down, 0.015f);

        // 3. MUSCLE MOMENT ARM
        muscleMomentArmLine.SetPosition(0, elbowPos);
        muscleMomentArmLine.SetPosition(1, muscleInsertionPoint);
        muscleMomentArmLine.enabled = true;

        cachedMuscleMomentArm = armTracker.GetMuscleMomentArm();

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

        if (handRightAngle != null) handRightAngle.enabled = false;
        if (forearmRightAngle != null) forearmRightAngle.enabled = false;
        if (muscleRightAngle != null) muscleRightAngle.enabled = false;
    }

    public void ToggleMomentArms(bool isOn)
    {
        showMomentArms = isOn;
    }

    // PUBLIC GETTERS for data panel
    public float GetHandMomentArm() => cachedHandMomentArm;
    public float GetForearmMomentArm() => cachedForearmMomentArm;
    public float GetMuscleMomentArm() => cachedMuscleMomentArm;
    public bool IsVisible() => showMomentArms;
}