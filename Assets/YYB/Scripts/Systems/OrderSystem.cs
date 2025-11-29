using UnityEngine;
using System.Collections.Generic;
using Alkuul.Domain;
using Alkuul.Core;

namespace Alkuul.Systems
{
    /// <summary>2차 감정 키워드 → 목표 1차 감정 벡터 변환</summary>
    public sealed class OrderSystem : MonoBehaviour
    {
        public Order CreateOrder(IEnumerable<SecondaryEmotionSO> keywords, Vector2 abvRange, float timeLimit)
        {
            EmotionVector result = default; int n = 0;
            foreach (var kw in keywords) { result = VectorOps.AddWeighted(result, kw.composition, 1f); n++; }
            if (n > 0) result = VectorOps.Normalize(result);

            return new Order { targetEmotions = result, abvRange = abvRange, timeLimit = timeLimit };
        }
    }
}
