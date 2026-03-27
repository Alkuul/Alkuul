using UnityEngine;
using Alkuul.Domain.Brewing;

namespace Alkuul.UI.Brewing
{
    public enum HoldGaugeMode
    {
        MaintainInZone,   // 블렌더: 적정 구간 유지
        ReleaseInZone     // 탄산: 적정 구간에서 손 떼기
    }

    [CreateAssetMenu(menuName = "Alkuul/Brewing/Technique Interaction Spec")]
    public class TechniqueInteractionSpec : ScriptableObject
    {
        [Header("Basic")]
        public TechniqueType techniqueType;
        public string displayName;

        [Header("Common")]
        public float timeLimit = 5f;
        [Range(0f, 1f)] public float successThreshold = 0.7f;

        [Header("Shake")]
        public int targetShakeCount = 8;
        public float minShakeDistance = 60f;
        public float visualShakeAmount = 18f;

        [Header("Stir")]
        public float targetRotationAmount = 720f;   // 총 회전량(도)
        public float stirRadiusMin = 60f;
        public float maxAcceptedAngleStep = 90f;

        [Header("Stir Visual")]
        public float visualOrbitRadius = 90f;
        public float visualFollowSpeed = 18f;

        [Header("Hold Gauge - Blender / Carbonation")]
        public HoldGaugeMode holdGaugeMode = HoldGaugeMode.MaintainInZone;
        public float gaugeFillSpeed = 0.7f;
        public float gaugeDrainSpeed = 0.35f;
        [Range(0f, 1f)] public float targetZoneMin01 = 0.45f;
        [Range(0f, 1f)] public float targetZoneMax01 = 0.65f;
        public float targetHoldTime = 1.3f;

        [Header("Rolling")]
        public int requiredRollPassCount = 4;
        public float pointDetectRadius = 45f;
        public float rollingFollowSpeed = 18f;

        [Header("Smoking / Timing")]
        public int targetHitCount = 3;
        public float markerSpeed01 = 1.2f;
        [Range(0f, 1f)] public float hitZoneMin01 = 0.42f;
        [Range(0f, 1f)] public float hitZoneMax01 = 0.58f;
    }
}