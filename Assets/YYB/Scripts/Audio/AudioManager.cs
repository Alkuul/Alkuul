using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Alkuul.Audio
{
    /// <summary>
    /// 게임 전역에서 사용하는 사운드 매니저.
    /// 씬 어디서든 AudioManager.Instance.Play(SoundId.XXX) 한 줄로 재생 가능.
    ///
    /// 설계:
    /// - 싱글톤, DontDestroyOnLoad
    /// - SFX는 풀(pool)에서 빈 AudioSource를 빌려서 재생
    /// - BGM은 별도 AudioSource로 재생 (페이드 인/아웃 가능)
    /// - 미니게임 루프 사운드는 별도 AudioSource를 빌려서 PlayLoop / StopLoop
    /// - SoundLibrary.Entry의 playStartTime / playEndTime 으로 재생 구간 설정 가능
    ///
    /// 사용 예:
    ///   AudioManager.Instance.Play(SoundId.SFX_GameClick);
    ///   AudioManager.Instance.PlayBGM(SoundId.BGM_Customer1);
    ///   var handle = AudioManager.Instance.PlayLoop(SoundId.SFX_Shaking);
    ///   AudioManager.Instance.StopLoop(handle);
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Library")]
        [SerializeField] private SoundLibrary library;

        [Header("Mixer (Optional)")]
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        [Header("Pool")]
        [Tooltip("동시에 재생 가능한 SFX 개수. 부족하면 자동 확장됨.")]
        [SerializeField] private int sfxPoolSize = 8;

        [Header("BGM")]
        [Tooltip("BGM 전환 시 페이드 시간 (초). 0이면 즉시 전환.")]
        [SerializeField] private float bgmFadeDuration = 0.6f;

        [Header("Master Volume")]
        [Tooltip("이 값은 GameSettingsStore의 마스터 볼륨과 별개. 보통 1로 두고 GameSettingsStore.SetMasterVolume()으로 조정.")]
        [Range(0f, 1f)]
        [SerializeField] private float masterMultiplier = 1f;

        [Header("Debug")]
        [SerializeField] private bool verboseLog = false;

        // BGM
        private AudioSource _bgmSource;
        private SoundId _currentBGM = SoundId.None;
        private Coroutine _bgmFadeRoutine;

        // SFX 풀
        private readonly List<AudioSource> _sfxPool = new();

        // 루프 사운드 핸들 관리
        private readonly Dictionary<int, AudioSource> _loopSources = new();
        private int _nextLoopHandle = 1;

        // 같은 SFX 너무 빨리 연속 재생 방지
        private readonly Dictionary<SoundId, float> _lastPlayTime = new();

        // 자동 정지 코루틴 추적 (재사용 시 이전 정지 코루틴을 취소하기 위해)
        private readonly Dictionary<AudioSource, Coroutine> _autoStopRoutines = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void InitializeAudioSources()
        {
            // BGM 전용 AudioSource
            var bgmGo = new GameObject("BGM Source");
            bgmGo.transform.SetParent(transform, false);
            _bgmSource = bgmGo.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;
            _bgmSource.outputAudioMixerGroup = bgmGroup;

            // SFX 풀
            for (int i = 0; i < sfxPoolSize; i++)
            {
                _sfxPool.Add(CreateSfxSource(i));
            }
        }

        private AudioSource CreateSfxSource(int index)
        {
            var go = new GameObject($"SFX Source {index}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.outputAudioMixerGroup = sfxGroup;
            return src;
        }

        // ===== Public API: SFX (one-shot) =====

        /// <summary>
        /// 일회성 효과음 재생. 같은 사운드를 너무 빨리 연속 호출하면 자동으로 무시함.
        /// SoundLibrary의 playStartTime / playEndTime 이 자동 적용됨.
        /// </summary>
        public void Play(SoundId id)
        {
            Play(id, 1f);
        }

        /// <summary>
        /// 일회성 효과음 재생 + 추가 볼륨 배율.
        /// </summary>
        public void Play(SoundId id, float volumeScale)
        {
            if (id == SoundId.None) return;
            if (library == null)
            {
                if (verboseLog) Debug.LogWarning("[AudioManager] Library is null.");
                return;
            }

            if (!library.TryGet(id, out var entry) || entry.clip == null)
            {
                if (verboseLog) Debug.LogWarning($"[AudioManager] No clip for {id}.");
                return;
            }

            // 너무 빠른 연속 재생 방지
            if (entry.minIntervalSec > 0f)
            {
                if (_lastPlayTime.TryGetValue(id, out var last))
                {
                    if (Time.unscaledTime - last < entry.minIntervalSec)
                        return;
                }
                _lastPlayTime[id] = Time.unscaledTime;
            }

            var src = GetFreeSfxSource();

            // 이전에 같은 소스에 자동 정지 코루틴이 걸려있으면 취소
            CancelAutoStop(src);

            src.clip = entry.clip;
            src.loop = false;
            src.volume = Mathf.Clamp01(entry.volume * volumeScale * masterMultiplier);
            src.pitch = 1f;

            // 재생 시작 시점 적용
            float startTime = Mathf.Clamp(entry.playStartTime, 0f, Mathf.Max(0f, entry.clip.length - 0.001f));
            src.time = startTime;

            src.Play();

            // 재생 종료 시점이 지정되었으면 자동 정지 코루틴 시작
            if (entry.playEndTime > 0f && entry.playEndTime > startTime)
            {
                float playDuration = entry.playEndTime - startTime;
                var routine = StartCoroutine(CoAutoStop(src, playDuration));
                _autoStopRoutines[src] = routine;

                if (verboseLog)
                    Debug.Log($"[AudioManager] Play {id} (start={startTime:0.00}s, end={entry.playEndTime:0.00}s, duration={playDuration:0.00}s, vol={src.volume:0.00})");
            }
            else if (verboseLog)
            {
                Debug.Log($"[AudioManager] Play {id} (start={startTime:0.00}s, vol={src.volume:0.00})");
            }
        }

        private IEnumerator CoAutoStop(AudioSource src, float waitSeconds)
        {
            yield return new WaitForSeconds(waitSeconds);

            if (src != null && src.isPlaying)
            {
                src.Stop();
            }

            _autoStopRoutines.Remove(src);
        }

        private void CancelAutoStop(AudioSource src)
        {
            if (_autoStopRoutines.TryGetValue(src, out var routine))
            {
                if (routine != null) StopCoroutine(routine);
                _autoStopRoutines.Remove(src);
            }
        }

        // ===== Public API: BGM =====

        /// <summary>
        /// BGM 재생. 같은 BGM이 이미 재생 중이면 아무것도 안 함.
        /// 다른 BGM이 재생 중이면 페이드 전환.
        /// </summary>
        public void PlayBGM(SoundId id)
        {
            if (id == SoundId.None)
            {
                StopBGM();
                return;
            }

            if (_currentBGM == id && _bgmSource.isPlaying)
                return;

            if (library == null) return;
            if (!library.TryGet(id, out var entry) || entry.clip == null)
            {
                if (verboseLog) Debug.LogWarning($"[AudioManager] No BGM clip for {id}.");
                return;
            }

            _currentBGM = id;

            if (_bgmFadeRoutine != null)
                StopCoroutine(_bgmFadeRoutine);

            _bgmFadeRoutine = StartCoroutine(CoFadeToBGM(entry));
            if (verboseLog) Debug.Log($"[AudioManager] PlayBGM {id}");
        }

        /// <summary>BGM 페이드아웃 후 정지.</summary>
        public void StopBGM()
        {
            if (_currentBGM == SoundId.None && !_bgmSource.isPlaying) return;

            _currentBGM = SoundId.None;
            if (_bgmFadeRoutine != null) StopCoroutine(_bgmFadeRoutine);
            _bgmFadeRoutine = StartCoroutine(CoFadeOutBGM());
        }

        private IEnumerator CoFadeToBGM(SoundLibrary.Entry entry)
        {
            float targetVolume = Mathf.Clamp01(entry.volume * masterMultiplier);

            // 이미 재생 중인 BGM 페이드아웃
            if (_bgmSource.isPlaying && bgmFadeDuration > 0f)
            {
                float startVol = _bgmSource.volume;
                float t = 0f;
                while (t < bgmFadeDuration)
                {
                    t += Time.unscaledDeltaTime;
                    _bgmSource.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeDuration);
                    yield return null;
                }
            }

            // 새 BGM 시작
            _bgmSource.clip = entry.clip;
            _bgmSource.loop = true;
            _bgmSource.volume = bgmFadeDuration > 0f ? 0f : targetVolume;

            // BGM도 시작 시점 적용 (보통 0이지만 일관성 위해)
            float startTime = Mathf.Clamp(entry.playStartTime, 0f, Mathf.Max(0f, entry.clip.length - 0.001f));
            _bgmSource.time = startTime;

            _bgmSource.Play();

            // 페이드인
            if (bgmFadeDuration > 0f)
            {
                float t = 0f;
                while (t < bgmFadeDuration)
                {
                    t += Time.unscaledDeltaTime;
                    _bgmSource.volume = Mathf.Lerp(0f, targetVolume, t / bgmFadeDuration);
                    yield return null;
                }
                _bgmSource.volume = targetVolume;
            }

            _bgmFadeRoutine = null;
        }

        private IEnumerator CoFadeOutBGM()
        {
            if (bgmFadeDuration <= 0f)
            {
                _bgmSource.Stop();
                _bgmSource.clip = null;
                _bgmFadeRoutine = null;
                yield break;
            }

            float startVol = _bgmSource.volume;
            float t = 0f;
            while (t < bgmFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _bgmSource.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeDuration);
                yield return null;
            }
            _bgmSource.Stop();
            _bgmSource.clip = null;
            _bgmFadeRoutine = null;
        }

        // ===== Public API: Loop (미니게임 루프) =====

        /// <summary>
        /// 루프 사운드 재생. 반환된 핸들로 나중에 StopLoop 호출해야 정지됨.
        /// </summary>
        public int PlayLoop(SoundId id)
        {
            if (id == SoundId.None) return 0;
            if (library == null) return 0;
            if (!library.TryGet(id, out var entry) || entry.clip == null)
            {
                if (verboseLog) Debug.LogWarning($"[AudioManager] No loop clip for {id}.");
                return 0;
            }

            var src = GetFreeSfxSource();
            CancelAutoStop(src);

            src.clip = entry.clip;
            src.loop = true;
            src.volume = Mathf.Clamp01(entry.volume * masterMultiplier);
            src.pitch = 1f;

            float startTime = Mathf.Clamp(entry.playStartTime, 0f, Mathf.Max(0f, entry.clip.length - 0.001f));
            src.time = startTime;

            src.Play();

            int handle = _nextLoopHandle++;
            _loopSources[handle] = src;

            if (verboseLog) Debug.Log($"[AudioManager] PlayLoop {id} -> handle {handle}");
            return handle;
        }

        /// <summary>PlayLoop가 반환한 핸들로 루프 사운드를 정지.</summary>
        public void StopLoop(int handle)
        {
            if (handle <= 0) return;
            if (!_loopSources.TryGetValue(handle, out var src)) return;

            src.Stop();
            src.clip = null;
            src.loop = false;
            _loopSources.Remove(handle);

            if (verboseLog) Debug.Log($"[AudioManager] StopLoop handle {handle}");
        }

        /// <summary>모든 루프 사운드 정지.</summary>
        public void StopAllLoops()
        {
            foreach (var kv in _loopSources)
            {
                if (kv.Value == null) continue;
                kv.Value.Stop();
                kv.Value.clip = null;
                kv.Value.loop = false;
            }
            _loopSources.Clear();
        }

        // ===== 내부 풀 관리 =====

        private AudioSource GetFreeSfxSource()
        {
            // 사용 가능한 소스 찾기
            for (int i = 0; i < _sfxPool.Count; i++)
            {
                var src = _sfxPool[i];
                if (src == null) continue;
                if (!src.isPlaying && !IsInLoopUse(src))
                    return src;
            }

            // 다 차있으면 풀 확장
            var extra = CreateSfxSource(_sfxPool.Count);
            _sfxPool.Add(extra);
            if (verboseLog) Debug.Log($"[AudioManager] SFX pool expanded to {_sfxPool.Count}");
            return extra;
        }

        private bool IsInLoopUse(AudioSource src)
        {
            foreach (var kv in _loopSources)
                if (kv.Value == src) return true;
            return false;
        }
    }
}