using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alkuul.Domain
{
    /// <summary>2차 감정 키워드 → 1차 감정 조합</summary>
    [CreateAssetMenu(menuName = "Alkuul/Emotion/Secondary")]
    public class SecondaryEmotionSO : ScriptableObject
    {
        public string id;
        public string displayName;       // 키워드 표시명
        public EmotionVector composition; // 1차 감정 비율(합=1.0 또는 100 가이드)
    }
}

