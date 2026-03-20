using UnityEngine;
using Alkuul.Domain.Brewing;

namespace Alkuul.UI.Brewing
{
    [CreateAssetMenu(menuName = "Alkuul/Brewing/Technique Interaction Spec")]
    public class TechniqueInteractionSpec : ScriptableObject
    {
        public TechniqueType techniqueType;
        public string displayName;

        [Header("Common")]
        public float timeLimit = 5f;
        public float successThreshold = 0.7f;

        [Header("Shake")]
        public int targetShakeCount = 8;
        public float minShakeDistance = 60f;
        public float visualShakeAmount = 18f;

        [Header("Stir")]
        public float targetRotationAmount = 720f; // 총 회전량(도 단위)
        public float stirRadiusMin = 60f;
        public float maxAcceptedAngleStep = 90f;

        [Header("Stir Visual")]
        public float visualOrbitRadius = 90f;
        public float visualFollowSpeed = 18f;

        [Header("Hold")]
        public float targetHoldTime = 2.5f;

        [Header("Timing")]
        public int targetSuccessCount = 3;
    }
}