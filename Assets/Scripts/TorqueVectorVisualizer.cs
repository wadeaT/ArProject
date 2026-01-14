using UnityEngine;

public class TorqueVectorVisualizer : MonoBehaviour
{
    public ArmTracker armTracker;

    [Header("Visualization Settings")]
    private bool showTorqueVector = false;
    public Color torqueColor = new Color(1f, 0.84f, 0f);
    public float torqueScale = 0.02f;

    private LineRenderer torqueArrow;
    private GameObject arrowHead;

    void Start()
    {
        CreateTorqueArrow();
    }

    void CreateTorqueArrow()
    {
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

        arrowHead = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arrowHead.transform.SetParent(transform);
        arrowHead.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);

        Renderer renderer = arrowHead.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = torqueColor;

        Destroy(arrowHead.GetComponent<Collider>());
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
        Vector3 handPos = armTracker.GetHandPos();

        Vector3 r = handPos - elbowPos;
        Vector3 F = Vector3.down;
        Vector3 torqueDirection = Vector3.Cross(r, F);

        if (torqueDirection.magnitude > 0.001f)
        {
            torqueDirection.Normalize();
        }
        else
        {
            torqueDirection = Vector3.forward;
        }

        float torqueMagnitude = armTracker.GetElbowTorque();
        float arrowLength = torqueMagnitude * torqueScale;
        arrowLength = Mathf.Clamp(arrowLength, 0.05f, 0.4f);

        Vector3 startPos = elbowPos;
        Vector3 endPos = startPos + torqueDirection * arrowLength;

        torqueArrow.SetPosition(0, startPos);
        torqueArrow.SetPosition(1, endPos);
        torqueArrow.enabled = true;

        arrowHead.transform.position = endPos;
        arrowHead.transform.rotation = Quaternion.LookRotation(torqueDirection);
        arrowHead.transform.Rotate(90, 0, 0);
        arrowHead.SetActive(true);
    }

    void HideTorqueVector()
    {
        if (torqueArrow != null) torqueArrow.enabled = false;
        if (arrowHead != null) arrowHead.SetActive(false);
    }

    public void ToggleTorqueVector(bool isOn)
    {
        showTorqueVector = isOn;
    }
}