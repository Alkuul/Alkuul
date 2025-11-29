using UnityEngine;

namespace Alkuul.Domain
{
    [System.Serializable]
    public struct EmotionVector
    {
        // 1차 감정 7축: 기쁨, 슬픔, 분노, 공포, 혐오, 놀람, 무감정
        public float joy;
        public float sadness;
        public float anger;
        public float fear;
        public float disgust;
        public float surprise;
        public float neutral; // 무감정
    }
}
