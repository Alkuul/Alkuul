using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Alkuul.UI
{
    public class TitleMenuController : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string introVideoSceneName = "IntroVideoScene";
        [SerializeField] private string coreSceneName = "CoreScene";

        [Header("New Game")]
        [SerializeField] private bool clearPlayerPrefsOnNewGame = false;

        [Header("UI Panels (optional)")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject continueComingSoonPanel;

        [Header("Optional UI")]
        [SerializeField] private Button continueButton;

        private void Start()
        {
            GameSettingsStore.LoadAndApply();

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            if (continueComingSoonPanel != null)
                continueComingSoonPanel.SetActive(false);

            RefreshContinueAvailability();
        }

        private void OnEnable()
        {
            RefreshContinueAvailability();
        }

        private void DestroyPersistentCoreIfExists()
        {
            var gameRoot = FindObjectOfType<GameRoot>(true);
            if (gameRoot != null)
                Destroy(gameRoot.gameObject);
        }

        public void OnClickNewGame()
        {
            DestroyPersistentCoreIfExists();

            float keepMasterVolume = GameSettingsStore.GetMasterVolume();
            bool keepFullscreen = GameSettingsStore.GetFullscreen();

            DayStartContinueSave.Clear();
            PrototypeEndingContext.Clear();

            if (clearPlayerPrefsOnNewGame)
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();

                // 설정은 유지
                GameSettingsStore.SetMasterVolume(keepMasterVolume);
                GameSettingsStore.SetFullscreen(keepFullscreen);
            }

            if (!string.IsNullOrWhiteSpace(introVideoSceneName))
                SceneManager.LoadScene(introVideoSceneName);
            else
                SceneManager.LoadScene(coreSceneName);
        }

        public void OnClickContinue()
        {
            DestroyPersistentCoreIfExists();

            if (!DayStartContinueSave.HasSave())
            {
                if (continueComingSoonPanel != null)
                    continueComingSoonPanel.SetActive(true);
                else
                    Debug.Log("[Title] No continue save found.");

                RefreshContinueAvailability();
                return;
            }

            PrototypeEndingContext.Clear();
            DayStartContinueSave.RequestContinueLoad();
            SceneManager.LoadScene(coreSceneName);
        }

        public void OnClickSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            else
                Debug.Log("[Title] settingsPanel is not assigned.");
        }

        public void OnClickQuit()
        {
#if UNITY_EDITOR
            Debug.Log("[Title] Quit (Editor).");
#else
            Application.Quit();
#endif
        }

        private void RefreshContinueAvailability()
        {
            if (continueButton != null)
                continueButton.interactable = DayStartContinueSave.HasSave();
        }
    }
}