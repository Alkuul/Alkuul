using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alkuul.Domain
{
    [System.Serializable]
    public struct Order
    {
        public EmotionVector targetEmotions; // 목표(1차 감정 벡터)
        public Vector2 abvRange;             // 희망 도수 범위(옵션)
        public float timeLimit;              // 잔당 제한시간(초)
    }
}

