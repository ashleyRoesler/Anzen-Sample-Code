using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioOptionsController : MonoBehaviour {

    public static AudioMixer AudioMixer;

    public OptionsUIController OptionsController;

    public static event System.Action<string, float> VolumeChanged;

    [Space]
    public Toggle MasterAudioToggle;
    public Slider MasterAudioSlider;

    [Min(0.0001f)]
    public float MasterAudioDefaultVolume = 1f;

    public Text CurrentMasterVolumeText;

    [Space]
    public Toggle MusicToggle;
    public Slider MusicSlider;

    [Min(0.0001f)]
    public float MusicDefaultVolume = 0.18f;

    public Text CurrentMusicVolumeText;

    [Space]
    public Toggle DialogueToggle;
    public Slider DialogueSlider;

    [Min(0.0001f)]
    public float DialogueDefaultVolume = 1;

    public Text CurrentDialogueVolumeText;

    [Space]
    public Toggle SFXToggle;
    public Slider SFXSlider;

    [Min(0.0001f)]
    public float SFXDefaultVolume = 0.32f;

    public Text CurrentSFXVolumeText;    

    private void Awake() {

        // master audio
        MasterAudioToggle.onValueChanged.AddListener(value => Toggle_Changed(MasterAudioSlider, "MasterVolume", value));
        MasterAudioSlider.onValueChanged.AddListener(value => Slider_Changed(CurrentMasterVolumeText, "MasterVolume", value));

        // music
        MusicToggle.onValueChanged.AddListener(value => Toggle_Changed(MusicSlider, "MusicVolume", value));
        MusicSlider.onValueChanged.AddListener(value => Slider_Changed(CurrentMusicVolumeText, "MusicVolume", value));

        // dialogue
        DialogueToggle.onValueChanged.AddListener(value => Toggle_Changed(DialogueSlider, "DialogueVolume", value));
        DialogueSlider.onValueChanged.AddListener(value => Slider_Changed(CurrentDialogueVolumeText, "DialogueVolume", value));

        // sfx
        SFXToggle.onValueChanged.AddListener(value => Toggle_Changed(SFXSlider, "SFXVolume", value));
        SFXSlider.onValueChanged.AddListener(value => Slider_Changed(CurrentSFXVolumeText, "SFXVolume", value));

        // defaults
        OptionsController.DefaultClicked += Options_DefaultClicked;
    }

    private void Start() {

        // start at default volume levels
        /// later, this might change so that it sets the volume to the player's saved preferred volume levels, but for now, this will work
        Options_DefaultClicked();

        // hide menu
        gameObject.SetActive(false);
    }

    private void OnDisable() {
        OptionsController.DefaultClicked -= Options_DefaultClicked;
    }

    private void Options_DefaultClicked() {
        MasterAudioToggle.isOn = true;
        MasterAudioSlider.value = MasterAudioDefaultVolume;

        MusicToggle.isOn = true;
        MusicSlider.value = MusicDefaultVolume;

        DialogueToggle.isOn = true;
        DialogueSlider.value = DialogueDefaultVolume;

        SFXToggle.isOn = true;
        SFXSlider.value = SFXDefaultVolume;
    }

    private void Toggle_Changed(Slider slider, string parameter, bool newValue) {
        float volume = newValue ? slider.value : 0.0001f;

        AudioMixer.SetFloat(parameter, Mathf.Log10(volume) * 20f);

        VolumeChanged?.Invoke(parameter, volume);
    }

    private void Slider_Changed(Text volumeText, string parameter, float newVolume) {
        volumeText.text = (newVolume * 100f).ToString("0");

        AudioMixer.SetFloat(parameter, Mathf.Log10(newVolume) * 20f);

        VolumeChanged?.Invoke(parameter, newVolume);
    }
}