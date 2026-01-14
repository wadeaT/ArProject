using UnityEngine;

public class CurvedTorqueArrow : MonoBehaviour
{
    public ArmTracker armTracker;

    private bool showCurvedArrow = false;
    private LineRenderer curvedLine;
    private GameObject arrowTip;

    [Header("Settings")]
    public float radius = 0.08f;
    public int curveSegments = 20;
    public Color torqueColor = Color.red;

    void Start()
    {
        CreateCurvedArrow();
    }

    void CreateCurvedArrow()
    {
        GameObject curveObj = new GameObject("CurvedTorqueArrow");
        curveObj.transform.SetParent(transform);
        curvedLine = curveObj.AddComponent<LineRenderer>();

        curvedLine.startWidth = 0.006f;
        curvedLine.endWidth = 0.006f;
        curvedLine.material = new Material(Shader.Find("Sprites/Default"));
        curvedLine.startColor = torqueColor;
        curvedLine.endColor = torqueColor;
        curvedLine.positionCount = curveSegments;
        curvedLine.useWorldSpace = true;

        arrowTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrowTip.transform.SetParent(transform);
        arrowTip.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        arrowTip.GetComponent<Renderer>().material.color = torqueColor;
        Destroy(arrowTip.GetComponent<Collider>());
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

        Vector3 armDirection = (handPos - elbowPos).normalized;
        Vector3 gravityForce = Vector3.down;
        Vector3 rotationAxis = Vector3.Cross(armDirection, gravityForce).normalized;

        if (rotationAxis.magnitude < 0.01f)
        {
            rotationAxis = Vector3.forward;
        }

        Vector3 startDir = armDirection;
        float arcAngleDegrees = 90f;
        bool clockwise = Vector3.Dot(rotationAxis, Vector3.Cross(armDirection, gravityForce)) > 0;

        for (int i = 0; i < curveSegments; i++)
        {
            float t = (float)i / (curveSegments - 1);
            float angle = t * arcAngleDegrees * (clockwise ? 1 : -1);

            Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis);
            Vector3 point = elbowPos + rotation * (startDir * radius);
            point += rotationAxis * 0.02f;

            curvedLine.SetPosition(i, point);
        }

        curvedLine.enabled = true;

        Vector3 lastPoint = curvedLine.GetPosition(curveSegments - 1);
        Vector3 secondToLast = curvedLine.GetPosition(curveSegments - 2);
        Vector3 tipDirection = (lastPoint - secondToLast).normalized;

        arrowTip.transform.position = lastPoint;
        arrowTip.transform.rotation = Quaternion.LookRotation(tipDirection);
        arrowTip.SetActive(true);
    }

    void HideCurvedArrow()
    {
        if (curvedLine != null) curvedLine.enabled = false;
        if (arrowTip != null) arrowTip.SetActive(false);
    }

    public void ToggleCurvedArrow(bool isOn)
    {
        showCurvedArrow = isOn;
    }
}