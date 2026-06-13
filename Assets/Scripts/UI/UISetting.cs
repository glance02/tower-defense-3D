using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UISetting : MonoBehaviour
{
    // 设置面板：调整相机灵敏度和 AudioMixer 音量，并用 PlayerPrefs 保存。
    [Header("Keyboard Sensetivity")]
    [SerializeField] private float minKeyboardSense = 60f;
    [SerializeField] private float maxKeyboardSense = 240f;
    [SerializeField] private string keyboardSenseParameter = "keyboardSens";
    [SerializeField] private Slider keyboardSenseSlider;
    [SerializeField] private TextMeshProUGUI keyboardSenseText;

    [Header("Mouse Sensetivity")]
    [SerializeField] private float minMouseSense = 1f;
    [SerializeField] private float maxMouseSense = 10f;
    [SerializeField] private string mouseSenseParameter = "mouseSens";
    [SerializeField] private Slider mouseSenseSlider;
    [SerializeField] private TextMeshProUGUI mouseSenseText;

    [Space]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 25;

    [Header("SFX Settings")]
    [SerializeField] private string sfxParameter;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxSliderText;

    [Header("BGM Settings")]
    [SerializeField] private string bgmParameter;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TextMeshProUGUI bgmSliderText;


    private CameraController cameraController;

    void Awake()
    {
        cameraController = FindFirstObjectByType<CameraController>();
    }

    public void SFXSliderValue(float changeValue)
    {
        // AudioMixer 使用分贝值，滑条 0-1 需要通过 log10 转换。
        float newValue = MathF.Log10(changeValue) * mixerMultiplier;
        audioMixer.SetFloat(sfxParameter, newValue);
        sfxSliderText.text = Mathf.RoundToInt(changeValue * 100) + "%";
    }

    public void BGMSliderValue(float changeValue)
    {
        // BGM 音量和 SFX 使用相同转换方式。
        float newValue = MathF.Log10(changeValue) * mixerMultiplier;
        audioMixer.SetFloat(bgmParameter, newValue);
        bgmSliderText.text = Mathf.RoundToInt(changeValue * 100) + "%";
    }

    public void AdjustKeyboardSense(float changeValue)
    {
        // 把 0-1 的滑条值映射到实际键盘移动速度范围。
        float newSense = Mathf.Lerp(minKeyboardSense, maxKeyboardSense, changeValue);
        cameraController.AdjustKeyboardMoveSpeed(newSense);
        keyboardSenseText.text = Mathf.RoundToInt(changeValue * 100) + "%";
    }

    public void AdjustMouseSense(float changeValue)
    {
        // 把 0-1 的滑条值映射到实际鼠标拖动速度范围。
        float newSense = Mathf.Lerp(minMouseSense, maxMouseSense, changeValue);
        cameraController.AdjustMouseMoveSpeed(newSense);
        mouseSenseText.text = Mathf.RoundToInt(changeValue * 100) + "%";
    }

    private void OnDisable()
    {
        // 关闭设置面板时保存当前设置。
        PlayerPrefs.SetFloat(keyboardSenseParameter, keyboardSenseSlider.value);
        PlayerPrefs.SetFloat(mouseSenseParameter, mouseSenseSlider.value);
        PlayerPrefs.SetFloat(sfxParameter, sfxSlider.value);
        PlayerPrefs.SetFloat(bgmParameter, bgmSlider.value);
    }

    private void OnEnable()
    {
        // 打开设置面板时读取本地保存值；没有保存则使用默认 .6。
        keyboardSenseSlider.value = PlayerPrefs.GetFloat(keyboardSenseParameter, .6f);
        mouseSenseSlider.value = PlayerPrefs.GetFloat(mouseSenseParameter, .6f);
        sfxSlider.value = PlayerPrefs.GetFloat(sfxParameter, .6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParameter, .6f);
    }
}
