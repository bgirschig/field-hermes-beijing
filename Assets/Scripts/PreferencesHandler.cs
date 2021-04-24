using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PreferencesHandler : MonoBehaviour
{
    [Header("Buttons")]
    public Button resetButton;
    
    [Header("Detector")]
    public Dropdown cameraSelector;
    public SharedWebcam webcam;

    [Header("Style")]
    public InputField lanternColorInput;
    public Renderer lantern;
    // controlled elements

    // Start is called before the first frame update
    void Start() {
        resetButton.interactable = false;
        resetButton.onClick.AddListener(reset);
        init();
    }

    void init() {
        cameraSelector.options.Clear();
        cameraSelector.AddOptions(webcam.devices);
        initOption("detector.camera_name", cameraSelector, (string val) => webcam.setCamera(val), "");
        initOption("style.lanternColor", lanternColorInput, (string val) => lantern.material.SetColor("_Color", colorFromHex(val)), "#009178");
    }

    // Initialize a float config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    void initOption(string name, Slider input, UnityAction<float> onChange, float defaultValue=0) {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetFloat(name, defaultValue);
        
        float value = PlayerPrefs.GetFloat(name);
        if (value != defaultValue) resetButton.interactable = true;
        
        input.value = value;
        onChange.Invoke(value);
        
        input.onValueChanged.AddListener(delegate {
            resetButton.interactable = true;
            onChange.Invoke(input.value);
            PlayerPrefs.SetFloat(name, input.value);
        });
    }

    // Initialize a float config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    void initOption(string name, Slider input, UnityAction<int> onChange, int defaultValue=0) {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetInt(name, defaultValue);
        
        int value = PlayerPrefs.GetInt(name);
        if (value != defaultValue) resetButton.interactable = true;

        input.value = value;
        onChange.Invoke(value);
        
        input.onValueChanged.AddListener(delegate {
            resetButton.interactable = true;
            onChange.Invoke((int)input.value);
            PlayerPrefs.SetInt(name, (int)input.value);
        });
    }

    // Initialize a string config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    void initOption(string name, InputField input, UnityAction<string> onChange, string defaultValue="") {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetString(name, defaultValue);
        
        string value = PlayerPrefs.GetString(name);
        if (value != defaultValue) resetButton.interactable = true;

        input.text = value;
        onChange.Invoke(value);
        
        input.onEndEdit.AddListener(delegate {
            resetButton.interactable = true;
            onChange.Invoke(input.text);
            PlayerPrefs.SetString(name, input.text);
        });
    }

    // Initialize a boolean config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    void initOption(string name, Toggle input, UnityAction<bool> onChange, bool defaultValue=false) {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetInt(name, defaultValue ? 1 : 0);
        
        bool value = PlayerPrefs.GetInt(name) == 1;
        if (value != defaultValue) resetButton.interactable = true;

        input.isOn = value;
        onChange.Invoke(value);
        
        input.onValueChanged.AddListener(delegate {
            resetButton.interactable = true;
            onChange.Invoke(input.isOn);
            PlayerPrefs.SetInt(name, input.isOn ? 1 : 0);
        });
    }

    // Initialize a dropdown config option: load from playerprefs, default value, update the global
    // 'resettable' and 'saveable' state, and change callback
    // Note: dropdown outputs an index, but to keep consistency if the order changes, we are saving
    // the label instead of its index in the options list.
    void initOption(string name, Dropdown input, UnityAction<string> onChange, string defaultValue="emulator") {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetString(name, defaultValue);
        
        string savedValue = PlayerPrefs.GetString(name);
        if (savedValue != defaultValue) resetButton.interactable = true;

        int valueIndex = input.options.FindIndex(item => { return item.text == savedValue; });
        input.value = valueIndex;
        onChange.Invoke(savedValue);
        
        input.onValueChanged.AddListener(delegate {
            resetButton.interactable = true;
            string newValue = input.options[input.value].text;
            onChange.Invoke(newValue);
            PlayerPrefs.SetString(name, newValue);
        });
    }

    // reset all options to their defaults
    public void reset() {
        PlayerPrefs.DeleteAll();
        resetButton.interactable = false;
        init();
    }

    // save the playerprefs
    // unused because playerprefs are saved automatically by unity,
    // making the "save" button rather confusing
    public void save() {
        PlayerPrefs.Save();
    }

    private static Color colorFromHex(string hexColor) {
        Color color = new Color();
        ColorUtility.TryParseHtmlString(hexColor, out color);
        return color;
    }
}