using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alkuul.Core
{
    public class CoreSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private string orderSceneName = "OrderScene";
        [SerializeField] private bool loadOrderOnStart = true;

        private void Start()
        {
            if (!loadOrderOnStart) return;

            // CoreScene 안의 GameRoot가 DontDestroyOnLoad 처리되었다면
            // LoadScene을 해도 시스템은 살아남음.
            SceneManager.LoadScene(orderSceneName);
        }
    }
}
