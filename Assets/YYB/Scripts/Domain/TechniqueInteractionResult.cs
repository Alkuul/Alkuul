using System;
using UnityEngine;

namespace Alkuul.Domain.Brewing
{
    [Serializable]
    public struct TechniqueInteractionResult
    {
        public TechniqueType TechniqueType;
        public bool Success;
        public float Quality01;     // 0~1
        public float Progress01;    // 목표 달성도
        public float ElapsedTime;
        public string Summary;

        public TechniqueInteractionResult(
            TechniqueType techniqueType,
            bool success,
            float quality01,
            float progress01,
            float elapsedTime,
            string summary)
        {
            TechniqueType = techniqueType;
            Success = success;
            Quality01 = Mathf.Clamp01(quality01);
            Progress01 = Mathf.Clamp01(progress01);
            ElapsedTime = Mathf.Max(0f, elapsedTime);
            Summary = summary;
        }
    }
}