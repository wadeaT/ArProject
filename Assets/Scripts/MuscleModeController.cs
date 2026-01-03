using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls switching between Biceps (curl) and Triceps (pulldown) modes.
/// 
/// BICEPS MODE (Curl):
///   - Resistance = Weight pulling DOWN (gravity)
///   - Muscle = Biceps pulling toward shoulder (flexion)
///   - Example: Bicep curl with dumbbell
///
/// TRICEPS MODE (Pulldown):
///   - Resistance = Cable pulling UP (tension)
///   - Muscle = Triceps pulling toward shoulder (extension)
///   - Example: Cable pulldown exercise
/// </summary>
public class MuscleModeController : MonoBehaviour
{
    [Header("References")]
    public ArmTracker armTracker;

    [Header("UI Elements")]
    public Toggle bicepsToggle;
    public Toggle tricepsToggle;
    public TMP_Text modeLabel;
    public TMP_Text resistanceLabel; // Shows "Weight ↓" or "Cable ↑"

    [Header("Visual Feedback")]
    public Image modeIndicator;
    public Color bicepsIndicatorColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);
    public Color tricepsIndicatorColor = new Color(0.6f, 0.2f, 0.8f, 0.3f);

    void Start()
    {
        if (bicepsToggle != null)
        {
            bicepsToggle.onValueChanged.AddListener(OnBicepsToggleChanged);
        }

        if (tricepsToggle != null)
        {
            tricepsToggle.onValueChanged.AddListener(OnTricepsToggleChanged);
        }

        // Initialize to biceps mode
        SetBicepsMode();
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS - Call from UI buttons or toggles
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// BICEPS MODE: Simulates a bicep curl
    /// - Weight in hand pulls DOWN (gravity)
    /// - Biceps muscle resists by pulling UP toward shoulder
    /// </summary>
    public void SetBicepsMode()
    {
        if (armTracker != null)
        {
            armTracker.SetBicepsMode();
        }

        UpdateUI(ArmTracker.MuscleMode.Biceps);
        Debug.Log("BICEPS MODE: Curl - Weight pulls DOWN, Biceps pulls UP");
    }

    /// <summary>
    /// TRICEPS MODE: Simulates a cable pulldown
    /// - Cable attached above pulls hand UP (tension)
    /// - Triceps muscle pushes DOWN to extend elbow
    /// </summary>
    public void SetTricepsMode()
    {
        if (armTracker != null)
        {
            armTracker.SetTricepsMode();
        }

        UpdateUI(ArmTracker.MuscleMode.Triceps);
        Debug.Log("TRICEPS MODE: Pulldown - Cable pulls UP, Triceps pushes DOWN");
    }

    // ═══════════════════════════════════════════════════════════════
    // TOGGLE LISTENERS
    // ═══════════════════════════════════════════════════════════════

    private void OnBicepsToggleChanged(bool isOn)
    {
        if (isOn) SetBicepsMode();
    }

    private void OnTricepsToggleChanged(bool isOn)
    {
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

        // Show resistance direction
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
}