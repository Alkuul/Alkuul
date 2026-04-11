using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Alkuul.UI
{
    public class GameSettingsPanelController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject rootPanel;

        [Header("Settings UI")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TMP_Text masterVolumeValueText;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Volume Icon")]
        [SerializeField] private Image volumeIconImage;
        [SerializeField] private Sprite[] volumeStepSprites; // 5장: 0,1,2,3,4단계

        [Header("Scene")]
        [SerializeField] private string titleSceneName = "TitleScene";

        private bool _suppressCallbacks;

        private void Awake()
        {
            if (rootPanel == null)
                rootPanel = gameObject;
        }

        private void OnEnable()
        {
            RefreshUIFromStore();
        }

        public void RefreshUIFromStore()
        {
            _suppressCallbacks = true;

            float normalizedVolume = GameSettingsStore.GetMasterVolume(); // 0~1
            bool fullscreen = GameSettingsStore.GetFullscreen();

            if (masterVolumeSlider != null)
                masterVolumeSlider.value = NormalizedToSliderValue(normalizedVolume);

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = fullscreen;

            RefreshVolumeText(normalizedVolume);
            RefreshVolumeIcon(normalizedVolume);

            _suppressCallbacks = false;
        }

        public void OnMasterVolumeChanged(float sliderValue)
        {
            if (_suppressCallbacks) return;

            float normalizedVolume = SliderValueToNormalized(sliderValue);

            GameSettingsStore.SetMasterVolume(normalizedVolume);
            RefreshVolumeText(normalizedVolume);
            RefreshVolumeIcon(normalizedVolume);
        }

        public void OnFullscreenChanged(bool value)
        {
            if (_suppressCallbacks) return;

            GameSettingsStore.SetFullscreen(value);
        }

        public void OnClickClose()
        {
            if (rootPanel != null)
                rootPanel.SetActive(false);
        }

        public void OnClickGoTitle()
        {
            var gameRoot = FindObjectOfType<GameRoot>(true);
            if (gameRoot != null)
                Destroy(gameRoot.gameObject);

            SceneManager.LoadScene(titleSceneName);
        }

        private float SliderValueToNormalized(float sliderValue)
        {
            if (masterVolumeSlider == null)
                return Mathf.Clamp01(sliderValue);

            float min = masterVolumeSlider.minValue;
            float max = masterVolumeSlider.maxValue;

            if (Mathf.Approximately(max, min))
                return 1f;

            // 슬라이더가 0~100이면 0~1로 정규화
            return Mathf.Clamp01((sliderValue - min) / (max - min));
        }

        private float NormalizedToSliderValue(float normalized)
        {
            if (masterVolumeSlider == null)
                return Mathf.Clamp01(normalized);

            float min = masterVolumeSlider.minValue;
            float max = masterVolumeSlider.maxValue;

            return Mathf.Lerp(min, max, Mathf.Clamp01(normalized));
        }

        private void RefreshVolumeText(float normalizedVolume)
        {
            if (masterVolumeValueText != null)
                masterVolumeValueText.text = Mathf.RoundToInt(Mathf.Clamp01(normalizedVolume) * 100f).ToString();
        }

        private void RefreshVolumeIcon(float normalizedVolume)
        {
            if (volumeIconImage == null || volumeStepSprites == null || volumeStepSprites.Length == 0)
                return;

            normalizedVolume = Mathf.Clamp01(normalizedVolume);

            int index;
            if (normalizedVolume <= 0f)
            {
                index = 0;
            }
            else if (normalizedVolume <= 0.25f)
            {
                index = 1;
            }
            else if (normalizedVolume <= 0.5f)
            {
                index = 2;
            }
            else if (normalizedVolume <= 0.75f)
            {
                index = 3;
            }
            else
            {
                index = 4;
            }

            index = Mathf.Clamp(index, 0, volumeStepSprites.Length - 1);
            volumeIconImage.sprite = volumeStepSprites[index];
        }
    }
}