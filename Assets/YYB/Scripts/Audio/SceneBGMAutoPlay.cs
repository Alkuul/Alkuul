using UnityEngine;

namespace Alkuul.Audio
{
    /// <summary>
    /// 씬에 배치되어 있으면 Start 시점에 지정한 BGM을 자동 재생.
    /// 타이틀 화면 등 EventBus 트리거 없이 BGM이 필요한 곳에 사용.
    /// AudioManager.Instance가 같은 씬/DontDestroyOnLoad에 살아있어야 동작.
    /// </summary>
    public class SceneBGMAutoPlay : MonoBehaviour
    {
        [SerializeField] private SoundId bgmId = SoundId.BGM_Intro;

        [Tooltip("이미 같은 BGM이 재생 중이면 재시작하지 않음. 보통 켜두는 게 자연스러움.")]
        [SerializeField] private bool skipIfAlreadyPlaying = true;

        private void Start()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[SceneBGMAutoPlay] AudioManager.Instance is null. Place AudioManager in the scene or in a persistent Core scene.");
                return;
            }

            // skipIfAlreadyPlaying은 AudioManager.PlayBGM이 이미 처리해줌
            // (같은 BGM이면 아무것도 안 함)
            AudioManager.Instance.PlayBGM(bgmId);
        }
    }
}
