using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls switching between Biceps (curl) and Triceps (pulldown) modes.
/// DEBUG VERSION - Extra logging to find the issue
/// </summary>
public class MuscleModeController : MonoBehaviour
{
    [Header("References")]
    public ArmTracker armTracker;

    [Header("UI Elements")]
    public Toggle bicepsToggle;
    public Toggle tricepsToggle;
    public TMP_Text modeLabel;
    public TMP_Text resistanceLabel;

    [Header("Visual Feedback")]
    public Image modeIndicator;
    public Color bicepsIndicatorColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);
    public Color tricepsIndicatorColor = new Color(0.6f, 0.2f, 0.8f, 0.3f);

    void Start()
    {
        Debug.Log("═══ MuscleModeController START ═══");

        // Check references
        if (armTracker == null)
        {
            Debug.LogError("MuscleModeController: armTracker is NULL! Drag ArmTracker to Inspector!");
            // Try to find it automatically
            armTracker = FindObjectOfType<ArmTracker>();
            if (armTracker != null)
            {
                Debug.Log("MuscleModeController: Found ArmTracker automatically!");
            }
        }
        else
        {
            Debug.Log("MuscleModeController: armTracker is assigned ✓");
        }

        if (bicepsToggle != null)
        {
            bicepsToggle.onValueChanged.AddListener(OnBicepsToggleChanged);
            Debug.Log("MuscleModeController: Biceps toggle connected ✓");
        }
        else
        {
            Debug.LogWarning("MuscleModeController: bicepsToggle is NULL!");
        }

        if (tricepsToggle != null)
        {
            tricepsToggle.onValueChanged.AddListener(OnTricepsToggleChanged);
            Debug.Log("MuscleModeController: Triceps toggle connected ✓");
        }
        else
        {
            Debug.LogWarning("MuscleModeController: tricepsToggle is NULL!");
        }

        // Initialize to biceps mode
        SetBicepsMode();
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════

    public void SetBicepsMode()
    {
        Debug.Log(">>> SetBicepsMode() called");

        if (armTracker != null)
        {
            Debug.Log(">>> Calling armTracker.SetBicepsMode()");
            armTracker.SetBicepsMode();
            Debug.Log($">>> ArmTracker mode is now: {armTracker.GetCurrentMuscleMode()}");
        }
        else
        {
            Debug.LogError(">>> armTracker is NULL - cannot set mode!");
            // Try to find it
            armTracker = FindObjectOfType<ArmTracker>();
            if (armTracker != null)
            {
                Debug.Log(">>> Found ArmTracker, retrying...");
                armTracker.SetBicepsMode();
            }
        }

        UpdateUI(ArmTracker.MuscleMode.Biceps);
    }

    public void SetTricepsMode()
    {
        Debug.Log(">>> SetTricepsMode() called");

        if (armTracker != null)
        {
            Debug.Log(">>> Calling armTracker.SetTricepsMode()");
            armTracker.SetTricepsMode();
            Debug.Log($">>> ArmTracker mode is now: {armTracker.GetCurrentMuscleMode()}");
        }
        else
        {
            Debug.LogError(">>> armTracker is NULL - cannot set mode!");
            // Try to find it
            armTracker = FindObjectOfType<ArmTracker>();
            if (armTracker != null)
            {
                Debug.Log(">>> Found ArmTracker, retrying...");
                armTracker.SetTricepsMode();
            }
        }

        UpdateUI(ArmTracker.MuscleMode.Triceps);
    }

    // ═══════════════════════════════════════════════════════════════
    // TOGGLE LISTENERS
    // ═══════════════════════════════════════════════════════════════

    private void OnBicepsToggleChanged(bool isOn)
    {
        Debug.Log($">>> OnBicepsToggleChanged: isOn = {isOn}");
        if (isOn) SetBicepsMode();
    }

    private void OnTricepsToggleChanged(bool isOn)
    {
        Debug.Log($">>> OnTricepsToggleChanged: isOn = {isOn}");
        if (isOn) SetTricepsMode();
    }

    // ═══════════════════════════════════════════════════════════════
    // UI UPDATE
    // ═══════════════════════════════════════════════════════════════

    private void UpdateUI(ArmTracker.MuscleMode mode)
    {
        if (modeLabel != null)
        {
            if (mode == ArmTracker.MuscleMode.Biceps)
            {
                modeLabel.text = "💪 BICEPS\n<size=70%>Curl Exercise</size>";
                modeLabel.color = Color.green;
            }
            else
            {
                modeLabel.text = "💪 TRICEPS\n<size=70%>Pulldown Exercise</size>";
                modeLabel.color = new Color(0.7f, 0.3f, 1f);
            }
        }

        if (resistanceLabel != null)
        {
            if (mode == ArmTracker.MuscleMode.Biceps)
            {
                resistanceLabel.text = "Resistance: Weight ↓";
                resistanceLabel.color = Color.red;
            }
            else
            {
                resistanceLabel.text = "Resistance: Cable ↑";
                resistanceLabel.color = Color.blue;
            }
        }

        if (modeIndicator != null)
        {
            modeIndicator.color = (mode == ArmTracker.MuscleMode.Biceps)
                ? bicepsIndicatorColor
                : tricepsIndicatorColor;
        }

        if (bicepsToggle != null && tricepsToggle != null)
        {
            bicepsToggle.SetIsOnWithoutNotify(mode == ArmTracker.MuscleMode.Biceps);
            tricepsToggle.SetIsOnWithoutNotify(mode == ArmTracker.MuscleMode.Triceps);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // DEBUG - Check status every frame (can be removed after debugging)
    // ═══════════════════════════════════════════════════════════════

    void Update()
    {
        // Press M key to manually check and log status
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("═══ MANUAL STATUS CHECK ═══");
            Debug.Log($"armTracker assigned: {armTracker != null}");
            if (armTracker != null)
            {
                Debug.Log($"Current muscle mode: {armTracker.GetCurrentMuscleMode()}");
            }
            Debug.Log($"bicepsToggle assigned: {bicepsToggle != null}");
            Debug.Log($"tricepsToggle assigned: {tricepsToggle != null}");
        }
    }
}