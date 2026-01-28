using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SIMPLE MODEL POSITION CONTROLLER
/// 
/// - Separate controls for Upper Arm and Forearm
/// - LARGER UI for easier touch interaction
/// - Toggle button always visible for easy close
/// </summary>
public class SimpleModelAdjuster : MonoBehaviour
{
    [Header("═══ REFERENCES ═══")]
    public TripoArmController tripoController;

    [Header("═══ DEFAULT VALUES ═══")]
    public float defaultScale = 1f;
    public float defaultThickness = 1f;

    // UI References
    private Button toggleButton;
    private GameObject slidersPanel;
    private CanvasGroup panelCanvasGroup;

    // Upper Arm Sliders
    private Slider upperPosXSlider, upperPosYSlider, upperPosZSlider;
    private Slider upperScaleSlider;

    // Forearm Sliders
    private Slider forearmPosXSlider, forearmPosYSlider, forearmPosZSlider;
    private Slider forearmScaleSlider;

    // Shared
    private Slider thicknessSlider;

    // Current values - Upper Arm
    private Vector3 upperArmOffset = Vector3.zero;
    private float upperArmScale = 1f;

    // Current values - Forearm
    private Vector3 forearmOffset = Vector3.zero;
    private float forearmScale = 1f;

    // Shared values
    private float thickness = 1f;

    private bool slidersVisible = false;

    void Start()
    {
        Debug.Log("SimpleModelAdjuster: Starting...");

        if (tripoController != null)
        {
            upperArmScale = tripoController.upperArmScaleMultiplier;
            forearmScale = tripoController.forearmScaleMultiplier;
            thickness = tripoController.thicknessMultiplier;
            upperArmOffset = tripoController.upperArmProportionalOffset;
            forearmOffset = tripoController.forearmProportionalOffset;
        }
        else
        {
            upperArmScale = defaultScale;
            forearmScale = defaultScale;
            thickness = defaultThickness;
        }

        LoadSettings();
        CreateUI();
        ApplyToController();
        HideSliders();

        Debug.Log("SimpleModelAdjuster: Setup complete!");
    }

    void CreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("AdjusterCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // ═══════════════════════════════════════════════════════════════
        // CREATE SLIDERS PANEL - LARGER SIZE
        // ═══════════════════════════════════════════════════════════════
        slidersPanel = new GameObject("SlidersPanel");
        slidersPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = slidersPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(1, 0);
        panelRect.anchoredPosition = new Vector2(-15, 80);
        panelRect.sizeDelta = new Vector2(420, 680); // BIGGER panel

        Image panelImage = slidersPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.92f);

        panelCanvasGroup = slidersPanel.AddComponent<CanvasGroup>();

        float yPos = -20f;
        float sliderSpacing = 55f;  // More space between sliders
        float sectionSpacing = 45f;

        // ═══════════════════════════════════════════════════════════════
        // CLOSE BUTTON (X) - BIGGER
        // ═══════════════════════════════════════════════════════════════
        CreateCloseButton(slidersPanel.transform, new Vector2(385, -30));

        // Title
        CreateLabel(slidersPanel.transform, "📐 Model Adjuster", 24, new Vector2(190, yPos), Color.cyan);
        yPos -= 45f;

        // ═══════════════════════════════════════════════════════════════
        // UPPER ARM SECTION
        // ═══════════════════════════════════════════════════════════════
        CreateLabel(slidersPanel.transform, "━━━ UPPER ARM ━━━", 18, new Vector2(210, yPos), new Color(1f, 0.6f, 0.2f));
        yPos -= 40f;

        CreateLabel(slidersPanel.transform, "Position X", 14, new Vector2(70, yPos), Color.white);
        upperPosXSlider = CreateSlider(slidersPanel.transform, -0.5f, 0.5f, upperArmOffset.x, new Vector2(255, yPos));
        upperPosXSlider.onValueChanged.AddListener(v => { upperArmOffset.x = v; ApplyToController(); });
        yPos -= sliderSpacing;

        CreateLabel(slidersPanel.transform, "Position Y", 14, new Vector2(70, yPos), Color.white);
        upperPosYSlider = CreateSlider(slidersPanel.transform, -0.5f, 0.5f, upperArmOffset.y, new Vector2(255, yPos));
        upperPosYSlider.onValueChanged.AddListener(v => { upperArmOffset.y = v; ApplyToController(); });
        yPos -= sliderSpacing;

        CreateLabel(slidersPanel.transform, "Position Z", 14, new Vector2(70, yPos), Color.white);
        upperPosZSlider = CreateSlider(slidersPanel.transform, -0.5f, 0.5f, upperArmOffset.z, new Vector2(255, yPos));
        upperPosZSlider.onValueChanged.AddListener(v => { upperArmOffset.z = v; ApplyToController(); });
        yPos -= sliderSpacing;

        CreateLabel(slidersPanel.transform, "Size", 14, new Vector2(70, yPos), Color.yellow);
        upperScaleSlider = CreateSlider(slidersPanel.transform, 0.1f, 5f, upperArmScale, new Vector2(255, yPos));
        upperScaleSlider.onValueChanged.AddListener(v => { upperArmScale = v; ApplyToController(); });
        yPos -= sectionSpacing + 10f;

        // ═══════════════════════════════════════════════════════════════
        // FOREARM SECTION
        // ═══════════════════════════════════════════════════════════════
        CreateLabel(slidersPanel.transform, "━━━ FOREARM ━━━", 18, new Vector2(210, yPos), new Color(0.4f, 0.8f, 1f));
        yPos -= 40f;

        CreateLabel(slidersPanel.transform, "Position X", 14, new Vector2(70, yPos), Color.white);
        forearmPosXSlider = CreateSlider(slidersPanel.transform, -0.5f, 0.5f, forearmOffset.x, new Vector2(255, yPos));
        forearmPosXSlider.onValueChanged.AddListener(v => { forearmOffset.x = v; ApplyToController(); });
        yPos -= sliderSpacing;

        CreateLabel(slidersPanel.transform, "Position Y", 14, new Vector2(70, yPos), Color.white);
        forearmPosYSlider = CreateSlider(slidersPanel.transform, -0.5f, 0.5f, forearmOffset.y, new Vector2(255, yPos));
        forearmPosYSlider.onValueChanged.AddListener(v => { forearmOffset.y = v; ApplyToController(); });
        yPos -= sliderSpacing;

        CreateLabel(slidersPanel.transform, "Position Z", 14, new Vector2(70, yPos), Color.white);
        forearmPosZSlider = CreateSlider(slidersPanel.transform, -0.5f, 0.5f, forearmOffset.z, new Vector2(255, yPos));
        forearmPosZSlider.onValueChanged.AddListener(v => { forearmOffset.z = v; ApplyToController(); });
        yPos -= sliderSpacing;

        CreateLabel(slidersPanel.transform, "Size", 14, new Vector2(70, yPos), Color.yellow);
        forearmScaleSlider = CreateSlider(slidersPanel.transform, 0.1f, 5f, forearmScale, new Vector2(255, yPos));
        forearmScaleSlider.onValueChanged.AddListener(v => { forearmScale = v; ApplyToController(); });
        yPos -= sectionSpacing + 10f;

        // ═══════════════════════════════════════════════════════════════
        // SHARED SETTINGS
        // ═══════════════════════════════════════════════════════════════
        CreateLabel(slidersPanel.transform, "━━━ BOTH ARMS ━━━", 18, new Vector2(210, yPos), Color.magenta);
        yPos -= 40f;

        CreateLabel(slidersPanel.transform, "Thickness", 14, new Vector2(70, yPos), Color.magenta);
        thicknessSlider = CreateSlider(slidersPanel.transform, 0.1f, 5f, thickness, new Vector2(255, yPos));
        thicknessSlider.onValueChanged.AddListener(v => { thickness = v; ApplyToController(); });
        yPos -= 60f;

        // ═══════════════════════════════════════════════════════════════
        // BUTTONS ROW - BIGGER
        // ═══════════════════════════════════════════════════════════════
        CreateResetButton(slidersPanel.transform, new Vector2(110, yPos));
        CreateSaveButton(slidersPanel.transform, new Vector2(310, yPos));

        // ═══════════════════════════════════════════════════════════════
        // CREATE TOGGLE BUTTON - BIGGER
        // ═══════════════════════════════════════════════════════════════
        GameObject buttonObj = new GameObject("AdjustModelButton");
        buttonObj.transform.SetParent(canvas.transform, false);

        RectTransform btnRect = buttonObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0);
        btnRect.anchorMax = new Vector2(1, 0);
        btnRect.pivot = new Vector2(1, 0);
        btnRect.anchoredPosition = new Vector2(-15, 15);
        btnRect.sizeDelta = new Vector2(180, 60); // BIGGER button

        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.5f, 0.8f, 0.95f);

        toggleButton = buttonObj.AddComponent<Button>();
        toggleButton.onClick.AddListener(ToggleSliders);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = btnTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMP_Text btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "⚙ Adjust Model";
        btnText.fontSize = 20;  // BIGGER font
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        Debug.Log("SimpleModelAdjuster: UI created with LARGE controls");
    }

    void CreateCloseButton(Transform parent, Vector2 position)
    {
        GameObject buttonObj = new GameObject("CloseButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(55, 55);  // BIGGER close button

        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);

        Button btn = buttonObj.AddComponent<Button>();
        btn.onClick.AddListener(HideSliders);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "✕";
        tmp.fontSize = 30;  // BIGGER X
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    void CreateLabel(Transform parent, string text, int fontSize, Vector2 position, Color color)
    {
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent, false);

        RectTransform rect = labelObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(380, 35);

        TMP_Text tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
    }

    Slider CreateSlider(Transform parent, float min, float max, float value, Vector2 position)
    {
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(parent, false);

        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(260, 40);  // WIDER and TALLER slider

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.2f);
        fillAreaRect.anchorMax = new Vector2(1, 0.8f);
        fillAreaRect.offsetMin = new Vector2(8, 0);
        fillAreaRect.offsetMax = new Vector2(-8, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 1f);

        // Handle area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(15, 0);
        handleAreaRect.offsetMax = new Vector2(-15, 0);

        // Handle - MUCH BIGGER for easy touch
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(40, 50);  // BIG handle
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;

        return slider;
    }

    void CreateResetButton(Transform parent, Vector2 position)
    {
        GameObject buttonObj = new GameObject("ResetButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(150, 55);  // BIGGER button

        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.7f, 0.25f, 0.25f);

        Button btn = buttonObj.AddComponent<Button>();
        btn.onClick.AddListener(ResetToDefault);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "↺ Reset";
        tmp.fontSize = 20;  // BIGGER font
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    void CreateSaveButton(Transform parent, Vector2 position)
    {
        GameObject buttonObj = new GameObject("SaveButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(150, 55);  // BIGGER button

        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.25f, 0.6f, 0.25f);

        Button btn = buttonObj.AddComponent<Button>();
        btn.onClick.AddListener(SaveSettings);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "💾 Save";
        tmp.fontSize = 20;  // BIGGER font
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    // ═══════════════════════════════════════════════════════════════════
    // SHOW / HIDE
    // ═══════════════════════════════════════════════════════════════════

    void ShowSliders()
    {
        slidersVisible = true;
        if (slidersPanel != null)
        {
            slidersPanel.SetActive(true);
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 1f;
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;
            }
        }
        Debug.Log("Sliders SHOWN");
    }

    public void HideSliders()
    {
        slidersVisible = false;
        if (slidersPanel != null)
        {
            slidersPanel.SetActive(false);
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }
        }
        Debug.Log("Sliders HIDDEN");
    }

    public void ToggleSliders()
    {
        if (slidersVisible)
            HideSliders();
        else
            ShowSliders();
    }

    // ═══════════════════════════════════════════════════════════════════
    // APPLY / SAVE / LOAD / RESET
    // ═══════════════════════════════════════════════════════════════════

    void ApplyToController()
    {
        if (tripoController == null)
        {
            Debug.LogWarning("SimpleModelAdjuster: No TripoArmController assigned!");
            return;
        }

        // Apply separate position offsets for each arm
        tripoController.upperArmProportionalOffset = upperArmOffset;
        tripoController.forearmProportionalOffset = forearmOffset;

        // Apply separate scales for each arm (using new fields in TripoArmController)
        tripoController.upperArmScaleMultiplier = upperArmScale;
        tripoController.forearmScaleMultiplier = forearmScale;

        // Shared thickness
        tripoController.thicknessMultiplier = thickness;
    }

    public void ResetToDefault()
    {
        upperArmOffset = Vector3.zero;
        forearmOffset = Vector3.zero;
        upperArmScale = defaultScale;
        forearmScale = defaultScale;
        thickness = defaultThickness;

        if (upperPosXSlider != null) upperPosXSlider.value = 0;
        if (upperPosYSlider != null) upperPosYSlider.value = 0;
        if (upperPosZSlider != null) upperPosZSlider.value = 0;
        if (upperScaleSlider != null) upperScaleSlider.value = defaultScale;

        if (forearmPosXSlider != null) forearmPosXSlider.value = 0;
        if (forearmPosYSlider != null) forearmPosYSlider.value = 0;
        if (forearmPosZSlider != null) forearmPosZSlider.value = 0;
        if (forearmScaleSlider != null) forearmScaleSlider.value = defaultScale;

        if (thicknessSlider != null) thicknessSlider.value = defaultThickness;

        ApplyToController();
        Debug.Log("Model positions reset to defaults");
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("UpperArmPosX", upperArmOffset.x);
        PlayerPrefs.SetFloat("UpperArmPosY", upperArmOffset.y);
        PlayerPrefs.SetFloat("UpperArmPosZ", upperArmOffset.z);
        PlayerPrefs.SetFloat("UpperArmScale", upperArmScale);

        PlayerPrefs.SetFloat("ForearmPosX", forearmOffset.x);
        PlayerPrefs.SetFloat("ForearmPosY", forearmOffset.y);
        PlayerPrefs.SetFloat("ForearmPosZ", forearmOffset.z);
        PlayerPrefs.SetFloat("ForearmScale", forearmScale);

        PlayerPrefs.SetFloat("ModelThickness", thickness);

        PlayerPrefs.Save();

        Debug.Log("Settings SAVED!");
    }

    void LoadSettings()
    {
        if (PlayerPrefs.HasKey("UpperArmScale"))
        {
            upperArmOffset.x = PlayerPrefs.GetFloat("UpperArmPosX", 0f);
            upperArmOffset.y = PlayerPrefs.GetFloat("UpperArmPosY", 0f);
            upperArmOffset.z = PlayerPrefs.GetFloat("UpperArmPosZ", 0f);
            upperArmScale = PlayerPrefs.GetFloat("UpperArmScale", defaultScale);

            forearmOffset.x = PlayerPrefs.GetFloat("ForearmPosX", 0f);
            forearmOffset.y = PlayerPrefs.GetFloat("ForearmPosY", 0f);
            forearmOffset.z = PlayerPrefs.GetFloat("ForearmPosZ", 0f);
            forearmScale = PlayerPrefs.GetFloat("ForearmScale", defaultScale);

            thickness = PlayerPrefs.GetFloat("ModelThickness", defaultThickness);

            Debug.Log("Settings LOADED");
        }
    }

    public void ClearSavedSettings()
    {
        PlayerPrefs.DeleteKey("UpperArmPosX");
        PlayerPrefs.DeleteKey("UpperArmPosY");
        PlayerPrefs.DeleteKey("UpperArmPosZ");
        PlayerPrefs.DeleteKey("UpperArmScale");
        PlayerPrefs.DeleteKey("ForearmPosX");
        PlayerPrefs.DeleteKey("ForearmPosY");
        PlayerPrefs.DeleteKey("ForearmPosZ");
        PlayerPrefs.DeleteKey("ForearmScale");
        PlayerPrefs.DeleteKey("ModelThickness");
        PlayerPrefs.DeleteKey("ModelPosX");
        PlayerPrefs.DeleteKey("ModelPosY");
        PlayerPrefs.DeleteKey("ModelPosZ");
        PlayerPrefs.DeleteKey("ModelScale");
        PlayerPrefs.Save();
        Debug.Log("All saved settings cleared!");
    }
}