using System.Collections.Generic;
using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.Systems
{
    /// <summary>도수/내성 기반 취함 포인트 계산 유틸</summary>
    public static class IntoxSystem
    {
        public static float GetToleranceMultiplier(Tolerance t)
        {
            return t switch
            {
                Tolerance.Weak => 1.25f,
                Tolerance.Strong => 0.75f,
                _ => 1f
            };
        }

        /// <summary>여러 잔의 Drink로부터 누적 취함 포인트 계산</summary>
        public static int ComputePoints(IEnumerable<Drink> drinks, Tolerance tolerance)
        {
            float totalAlcohol = 0f;

            foreach (var d in drinks)
            {
                // 도수(%) * ml / 100 = 순수 알코올량(ml 비례)
                totalAlcohol += d.finalABV * d.totalMl / 100f;
            }

            float mul = GetToleranceMultiplier(tolerance);
            return Mathf.RoundToInt(totalAlcohol * mul);
        }

        /// <summary>취함 단계(1~5)</summary>
        public static int GetStage(int points)
        {
            if (points >= 101) return 5; // 오버
            if (points >= 80) return 4; // 만취
            if (points >= 60) return 3; // 취함
            if (points >= 30) return 2; // 약취
            return 1;                     // 기본
        }
    }
}
