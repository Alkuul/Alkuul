using UnityEngine;
using Alkuul.Core;

namespace Alkuul.Audio
{
    /// <summary>
    /// "손님 받기" 클릭 횟수를 기준으로 BGM을 자동 전환하는 컴포넌트.
    ///
    /// 동작:
    /// - 1번째 손님 받기 클릭 = 튜토리얼 손님 → BGM_Intro 유지
    /// - 2번째 손님 받기 클릭 = 시퀀스 인덱스 0 → BGM_Customer1
    /// - 3번째 = BGM_Customer2
    /// - 4번째 = BGM_Customer3
    /// - 5번째 이후 = 시퀀스 끝 → BGM_Intro 복귀
    ///
    /// EventBus.OnDayStarted 도 구독해서 새 Day 시작 시 BGM_Intro 재호출 (같은 BGM이면 무시됨).
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public class AudioBindings : MonoBehaviour
    {
        [Header("Tutorial")]
        [Tooltip("처음 N번의 '손님 받기' 클릭은 튜토리얼로 간주하고 BGM_Intro 유지. 보통 1.")]
        [SerializeField] private int tutorialCustomerClickCount = 1;

        [Header("Customer BGM Sequence")]
        [Tooltip("튜토리얼 이후 손님 받기 클릭 순서대로 재생할 BGM. 시퀀스를 다 쓰면 BGM_Intro로 복귀.")]
        [SerializeField]
        private SoundId[] customerBgmSequence = new[]
        {
            SoundId.BGM_Customer1,
            SoundId.BGM_Customer2,
            SoundId.BGM_Customer3,
        };

        [Header("Default BGM")]
        [Tooltip("Day 시작 / 시퀀스 종료 후 복귀할 기본 BGM")]
        [SerializeField] private SoundId defaultBgm = SoundId.BGM_Intro;

        [Header("Debug")]
        [SerializeField] private bool verboseLog = false;

        // "손님 받기" 클릭 누적 횟수
        private int _customerClickCount;

        public int CustomerClickCount => _customerClickCount;

        private void OnEnable()
        {
            EventBus.OnDayStarted += HandleDayStarted;
        }

        private void OnDisable()
        {
            EventBus.OnDayStarted -= HandleDayStarted;
        }

        private void HandleDayStarted()
        {
            // Day 시작 시 BGM_Intro로. 같은 BGM 재생 중이면 AudioManager가 무시함
            if (verboseLog) Debug.Log($"[AudioBindings] Day started -> {defaultBgm}");
            PlayBGMSafe(defaultBgm);
        }

        // ===== Public API: "손님 받기" 클릭 시 외부에서 호출 =====

        /// <summary>
        /// "손님 받기" 버튼이 클릭되어 손님이 입장할 때마다 호출.
        /// 클릭 누적 횟수에 따라 자동으로 BGM 결정.
        /// </summary>
        public void OnCustomerEntered()
        {
            _customerClickCount++;

            if (verboseLog)
                Debug.Log($"[AudioBindings] Customer entered. Click count = {_customerClickCount}");

            // 튜토리얼 클릭 범위 (1, 2, ..., tutorialCustomerClickCount)
            if (_customerClickCount <= tutorialCustomerClickCount)
            {
                if (verboseLog)
                    Debug.Log($"[AudioBindings] Click {_customerClickCount} is tutorial. Keeping {defaultBgm}.");
                // 명시적으로 default BGM 재생 (이미 재생 중이면 무시됨)
                PlayBGMSafe(defaultBgm);
                return;
            }

            // 튜토리얼 이후 시퀀스 인덱스
            int seqIndex = _customerClickCount - tutorialCustomerClickCount - 1;

            if (customerBgmSequence == null || customerBgmSequence.Length == 0)
            {
                if (verboseLog) Debug.LogWarning("[AudioBindings] customerBgmSequence is empty.");
                return;
            }

            if (seqIndex >= customerBgmSequence.Length)
            {
                // 시퀀스 끝 → BGM_Intro 복귀
                if (verboseLog)
                    Debug.Log($"[AudioBindings] Sequence exhausted (index {seqIndex}). Returning to {defaultBgm}.");
                PlayBGMSafe(defaultBgm);
                return;
            }

            SoundId bgm = customerBgmSequence[seqIndex];
            if (verboseLog)
                Debug.Log($"[AudioBindings] Click {_customerClickCount} -> sequence index {seqIndex} -> {bgm}");
            PlayBGMSafe(bgm);
        }

        /// <summary>
        /// 새 게임 시작 등에서 카운트를 리셋하고 싶을 때 호출.
        /// </summary>
        public void ResetCounter()
        {
            _customerClickCount = 0;
            if (verboseLog) Debug.Log("[AudioBindings] Counter reset.");
        }

        // ===== 헬퍼 =====

        private static void PlayBGMSafe(SoundId id)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[AudioBindings] AudioManager.Instance is null.");
                return;
            }
            AudioManager.Instance.PlayBGM(id);
        }
    }
}