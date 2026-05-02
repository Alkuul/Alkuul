using System;
using System.Collections.Generic;
using UnityEngine;

namespace Alkuul.Audio
{
    /// <summary>
    /// SoundId → AudioClip + 볼륨 배율 매핑.
    /// ScriptableObject로 만들어 에디터에서 인스펙터로 편집한다.
    /// 각 사운드의 볼륨/재생구간을 보정할 수 있다.
    /// </summary>
    [CreateAssetMenu(menuName = "Alkuul/Audio/Sound Library", fileName = "SoundLibrary")]
    public class SoundLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("이 항목이 어떤 사운드인지 식별하는 ID")]
            public SoundId id = SoundId.None;

            [Tooltip("실제 재생할 오디오 클립")]
            public AudioClip clip;

            [Tooltip("이 사운드 고유의 볼륨 배율 (0~1). 파일별 음량 차이 보정용.")]
            [Range(0f, 1f)]
            public float volume = 1f;

            [Tooltip("재생 시작 시점(초). 0이면 처음부터. 예: 0.7이면 0.7초 지점부터 재생.")]
            [Min(0f)]
            public float playStartTime = 0f;

            [Tooltip("재생 종료 시점(초). 0이면 클립 끝까지. 예: 0.9면 0.9초에서 자동 정지.")]
            [Min(0f)]
            public float playEndTime = 0f;

            [Tooltip("BGM이나 미니게임 루프 사운드처럼 반복 재생할지 여부 (메모용)")]
            public bool loop;

            [Tooltip("같은 사운드가 동시에 여러 번 호출됐을 때 너무 가까운 시간차로는 다시 재생하지 않는 쿨다운(초). 0이면 비활성.")]
            [Range(0f, 1f)]
            public float minIntervalSec = 0.03f;
        }

        [Header("Sound Entries")]
        [SerializeField] private List<Entry> entries = new();

        // 런타임 캐시 (빠른 조회용)
        private Dictionary<SoundId, Entry> _cache;

        public IReadOnlyList<Entry> Entries => entries;

        /// <summary>SoundId로 Entry를 가져온다. 없으면 false.</summary>
        public bool TryGet(SoundId id, out Entry entry)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out entry);
        }

        private void EnsureCache()
        {
            if (_cache != null) return;

            _cache = new Dictionary<SoundId, Entry>(entries.Count);
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (e.id == SoundId.None) continue;
                if (_cache.ContainsKey(e.id))
                {
                    Debug.LogWarning($"[SoundLibrary] Duplicate SoundId: {e.id}. Keeping first entry.");
                    continue;
                }
                _cache[e.id] = e;
            }
        }

        /// <summary>에디터에서 entries가 변경되면 캐시 무효화.</summary>
        private void OnValidate()
        {
            _cache = null;
        }

        /// <summary>런타임에 인스펙터에서 값을 바꿔도 즉시 반영되도록 캐시 무효화.</summary>
        public void RebuildCache()
        {
            _cache = null;
            EnsureCache();
        }
    }
}