using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Alkuul.Domain;
using Alkuul.Core;

namespace Alkuul.Systems
{
    /// <summary>한 잔 평가(감정 일치 + 태그/얼음 보정)</summary>
    [CreateAssetMenu(menuName = "Alkuul/ScoringService")]
    public sealed class ScoringService : ScriptableObject
    {
        [Header("Bonuses")]
        public float techniqueMatch = 5f;
        public float glassMatch = 5f;
        public float garnish1 = 5f, garnish2 = 2f, garnish3 = 3f;
        public float iceLike = 15f, iceDislike = -10f;

        [Header("Tip")]
        [Tooltip("기본 1잔당 팁(100점 기준)")]
        public float baseTipPerDrink = 50f;

        [Header("Thresholds")]
        [Tooltip("이 값 미만이면 손님이 바로 떠남")]
        public float lowLeaveThresh = 40f;

        private static readonly string[] EmotionNames =
        {
            "joy", "sadness", "anger", "fear", "disgust", "surprise", "neutral"
        };

        /// <summary>주문 벡터에서 메이저 감정 태그(동률이면 여러개)</summary>
        private static string[] GetMajorEmotionTags(EmotionVector v, float tol = 0.01f)
        {
            float[] vals =
            {
                v.joy, v.sadness, v.anger, v.fear, v.disgust, v.surprise, v.neutral
            };
            float max = vals.Max();
            if (max <= 0f) return System.Array.Empty<string>();

            var list = new List<string>();
            for (int i = 0; i < vals.Length; i++)
            {
                if (Mathf.Abs(vals[i] - max) <= tol && vals[i] > 0f)
                    list.Add(EmotionNames[i]);
            }
            return list.ToArray();
        }

        private static bool HasAnyTag(string[] source, string[] majors)
        {
            if (source == null || majors == null) return false;
            return source.Any(t => majors.Contains(t));
        }

        private static int CountHits(string[] source, string[] majors)
        {
            if (source == null || majors == null) return 0;
            return source.Count(t => majors.Contains(t));
        }

        public DrinkResult Evaluate(
            Order o,
            Drink d,
            ServeSystem.Meta meta,
            CustomerProfile customer
        )
        {
            // 기본 일치율: Overlap 기반 (0~1)
            float overlap = VectorOps.Overlap(o.targetEmotions, d.emotions);
            float match = overlap * 100f;

            // 메이저 감정 태그들
            var majorTags = GetMajorEmotionTags(o.targetEmotions);

            float bonus = 0f;

            // 조주기법 / 잔 보너스
            if (HasAnyTag(meta.techniqueTags, majorTags))
                bonus += techniqueMatch;

            if (HasAnyTag(meta.glassTags, majorTags))
                bonus += glassMatch;

            // 가니쉬: 맞는 개수에 따라 1,2,3 단계 보너스
            int garnishHits = CountHits(meta.garnishTags, majorTags);
            if (garnishHits >= 1) bonus += garnish1;
            if (garnishHits >= 2) bonus += garnish2;
            if (garnishHits >= 3) bonus += garnish3;

            // 얼음 선호/비선호
            if (meta.usesIce && customer.icePreference == IcePreference.Like)
                bonus += iceLike;
            else if (meta.usesIce && customer.icePreference == IcePreference.Dislike)
                bonus += iceDislike;

            // raw 점수는 100 초과 허용 (팁 계산에 사용)
            float rawScore = Mathf.Max(match + bonus, 0f);

            // UI에 보여줄 만족도는 0~100 클램프
            float displayScore = Mathf.Clamp(rawScore, 0f, 135f);

            // 팁: 100점일 때 baseTipPerDrink, 150점이면 1.5배, 이런 느낌
            int tip = Mathf.FloorToInt(baseTipPerDrink * (rawScore / 100f));

            return new DrinkResult
            {
                satisfaction = displayScore,
                satisfactionRaw = rawScore,
                tip = tip,
                customerLeft = displayScore < lowLeaveThresh
            };
        }
    }
}
