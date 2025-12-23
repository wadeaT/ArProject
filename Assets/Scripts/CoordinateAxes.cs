using UnityEngine;
using TMPro;

public class CoordinateAxes : MonoBehaviour
{
    public Transform anchorPoint; // Where to place the axes (shoulder)

    [Header("Settings")]
    public float axisLength = 0.1f;
    private bool showAxes = true;

    private LineRenderer xAxis, yAxis, zAxis;
    private TextMeshPro xLabel, yLabel, zLabel;

    void Start()
    {
        CreateAxes();
        CreateLabels();
    }

    void CreateAxes()
    {
        // X axis (Red)
        xAxis = CreateAxisLine("X_Axis", Color.red);

        // Y axis (Green)
        yAxis = CreateAxisLine("Y_Axis", Color.green);

        // Z axis (Blue)
        zAxis = CreateAxisLine("Z_Axis", Color.blue);
    }

    LineRenderer CreateAxisLine(string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);

        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.startWidth = 0.003f;
        line.endWidth = 0.003f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.positionCount = 2;

        return line;
    }

    void CreateLabels()
    {
        xLabel = CreateAxisLabel("X", Color.red);
        yLabel = CreateAxisLabel("Y", Color.green);
        zLabel = CreateAxisLabel("Z", Color.blue);
    }

    TextMeshPro CreateAxisLabel(string text, Color color)
    {
        GameObject labelObj = new GameObject($"Label_{text}");
        labelObj.transform.SetParent(transform);

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 0.2f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;

        labelObj.AddComponent<Billboard>();

        return tmp;
    }

    void Update()
    {
        if (showAxes && anchorPoint != null)
        {
            DrawAxes();
        }
        else
        {
            HideAxes();
        }
    }

    void DrawAxes()
    {
        Vector3 origin = anchorPoint.position;

        // X axis (Right) - RED
        xAxis.SetPosition(0, origin);
        xAxis.SetPosition(1, origin + Vector3.right * axisLength);
        xLabel.transform.position = origin + Vector3.right * (axisLength + 0.02f);
        xAxis.enabled = true;
        xLabel.gameObject.SetActive(true);

        // Y axis (Up) - GREEN
        yAxis.SetPosition(0, origin);
        yAxis.SetPosition(1, origin + Vector3.up * axisLength);
        yLabel.transform.position = origin + Vector3.up * (axisLength + 0.02f);
        yAxis.enabled = true;
        yLabel.gameObject.SetActive(true);

        // Z axis (Forward) - BLUE
        zAxis.SetPosition(0, origin);
        zAxis.SetPosition(1, origin + Vector3.forward * axisLength);
        zLabel.transform.position = origin + Vector3.forward * (axisLength + 0.02f);
        zAxis.enabled = true;
        zLabel.gameObject.SetActive(true);
    }

    void HideAxes()
    {
        if (xAxis != null) xAxis.enabled = false;
        if (yAxis != null) yAxis.enabled = false;
        if (zAxis != null) zAxis.enabled = false;
        if (xLabel != null) xLabel.gameObject.SetActive(false);
        if (yLabel != null) yLabel.gameObject.SetActive(false);
        if (zLabel != null) zLabel.gameObject.SetActive(false);
    }

    // PUBLIC METHOD FOR TOGGLE - This is what Unity needs!
    public void ToggleAxes(bool isOn)
    {
        showAxes = isOn;
        Debug.Log($"Coordinate Axes toggled: {isOn}");
    }
}