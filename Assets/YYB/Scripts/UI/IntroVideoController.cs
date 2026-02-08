using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Alkuul.UI
{
    public class IntroVideoController : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer player;
        [SerializeField] private bool allowSkip = true;

        [Header("Scene")]
        [SerializeField] private string coreSceneName = "CoreScene";

        private bool _loading;

        private void Awake()
        {
            if (player == null) player = GetComponent<VideoPlayer>();
        }

        private void Start()
        {
            if (player == null)
            {
                Debug.LogWarning("[IntroVideo] VideoPlayer not found. Go to CoreScene.");
                LoadCore();
                return;
            }

            player.loopPointReached += OnVideoFinished;
            player.errorReceived += OnVideoError;

            player.Play();
        }

        private void Update()
        {
            if (!allowSkip || _loading) return;

            // ½ºÅµÅ°´Â ¿øÇÏ´Â´ë·Î ¹Ù²ãµµ µÊ
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                LoadCore();
            }
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            LoadCore();
        }

        private void OnVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogWarning($"[IntroVideo] Video error: {msg}");
            LoadCore();
        }

        private void LoadCore()
        {
            if (_loading) return;
            _loading = true;
            SceneManager.LoadScene(coreSceneName);
        }
    }
}
