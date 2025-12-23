using UnityEngine;

public class LeverArmVisualizer : MonoBehaviour
{
    public ArmTracker armTracker;

    private LineRenderer handLeverLine;
    private LineRenderer forearmLeverLine;

    void Start()
    {
        // Create line for hand lever arm
        GameObject handLine = new GameObject("HandLeverLine");
        handLine.transform.SetParent(transform);
        handLeverLine = handLine.AddComponent<LineRenderer>();
        SetupLine(handLeverLine, Color.cyan);

        // Create line for forearm lever arm
        GameObject forearmLine = new GameObject("ForearmLeverLine");
        forearmLine.transform.SetParent(transform);
        forearmLeverLine = forearmLine.AddComponent<LineRenderer>();
        SetupLine(forearmLeverLine, Color.magenta);
    }

    void SetupLine(LineRenderer line, Color color)
    {
        line.startWidth = 0.003f;
        line.endWidth = 0.003f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.positionCount = 2;

        // Make dashed line effect
        line.textureMode = LineTextureMode.Tile;
    }

    void Update()
    {
        if (armTracker.AllTracked())
        {
            DrawLeverArms();
        }
        else
        {
            handLeverLine.enabled = false;
            forearmLeverLine.enabled = false;
        }
    }

    void DrawLeverArms()
    {
        Vector3 elbowPos = armTracker.GetElbowPos();
        Vector3 handPos = armTracker.GetHandPos();
        Vector3 forearmCenter = (elbowPos + handPos) / 2f;

        // Hand lever arm (horizontal line from elbow to hand)
        handLeverLine.SetPosition(0, elbowPos);
        handLeverLine.SetPosition(1, new Vector3(handPos.x, elbowPos.y, handPos.z));
        handLeverLine.enabled = true;

        // Forearm lever arm (horizontal line from elbow to forearm center)
        forearmLeverLine.SetPosition(0, elbowPos);
        forearmLeverLine.SetPosition(1, new Vector3(forearmCenter.x, elbowPos.y, forearmCenter.z));
        forearmLeverLine.enabled = true;
    }
}