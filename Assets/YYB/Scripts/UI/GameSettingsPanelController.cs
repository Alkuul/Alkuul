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

            float volume = GameSettingsStore.GetMasterVolume();
            bool fullscreen = GameSettingsStore.GetFullscreen();

            if (masterVolumeSlider != null)
                masterVolumeSlider.value = volume;

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = fullscreen;

            RefreshVolumeText(volume);

            _suppressCallbacks = false;
        }

        public void OnMasterVolumeChanged(float value)
        {
            if (_suppressCallbacks) return;

            GameSettingsStore.SetMasterVolume(value);
            RefreshVolumeText(value);
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

        private void RefreshVolumeText(float value)
        {
            if (masterVolumeValueText != null)
                masterVolumeValueText.text = $"{Mathf.RoundToInt(value * 100f)}";
        }
    }
}