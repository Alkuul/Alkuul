using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alkuul.UI
{
    public class TitleMenuController : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string introVideoSceneName = "IntroVideoScene";
        [SerializeField] private string coreSceneName = "CoreScene"; // 인트로 씬 없이 바로 갈 때 대비

        [Header("New Game")]
        [SerializeField] private bool clearPlayerPrefsOnNewGame = false;

        [Header("UI Panels (optional)")]
        [SerializeField] private GameObject settingsPanel;  // 있으면 켜고/끄기
        [SerializeField] private GameObject continueComingSoonPanel; // "준비중" 팝업 있으면

        public void OnClickNewGame()
        {
            if (clearPlayerPrefsOnNewGame)
            {
                // TODO: 나중에 세이브 키를 정하면 특정 키만 지우는 걸 추천
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
            }

            // 인트로 비디오 씬으로 이동
            if (!string.IsNullOrWhiteSpace(introVideoSceneName))
                SceneManager.LoadScene(introVideoSceneName);
            else
                SceneManager.LoadScene(coreSceneName);
        }

        public void OnClickContinue()
        {
            // 지금은 스텁 처리
            if (continueComingSoonPanel != null)
            {
                continueComingSoonPanel.SetActive(true);
            }
            else
            {
                Debug.Log("[Title] Continue is not implemented yet.");
            }
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
    }
}
