using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.Core
{
    /// <summary>감정 벡터 합성/정규화/거리 계산</summary>
    public static class VectorOps
    {
        public static EmotionVector AddWeighted(EmotionVector acc, EmotionVector x, float w)
        {
            acc.joy += x.joy * w;
            acc.sadness += x.sadness * w;
            acc.anger += x.anger * w;
            acc.fear += x.fear * w;
            acc.disgust += x.disgust * w;
            acc.surprise += x.surprise * w;
            acc.neutral += x.neutral * w;
            return acc;
        }

        public static EmotionVector Normalize(EmotionVector v)
        {
            float sum = v.joy + v.sadness + v.anger + v.fear
                        + v.disgust + v.surprise + v.neutral;
            if (sum <= 0.0001f) return v;
            float inv = 1f / sum;

            v.joy *= inv;
            v.sadness *= inv;
            v.anger *= inv;
            v.fear *= inv;
            v.disgust *= inv;
            v.surprise *= inv;
            v.neutral *= inv;

            return v;
        }

        public static float L1(EmotionVector a, EmotionVector b)
        {
            return Mathf.Abs(a.joy - b.joy)
                 + Mathf.Abs(a.sadness - b.sadness)
                 + Mathf.Abs(a.anger - b.anger)
                 + Mathf.Abs(a.fear - b.fear)
                 + Mathf.Abs(a.disgust - b.disgust)
                 + Mathf.Abs(a.surprise - b.surprise)
                 + Mathf.Abs(a.neutral - b.neutral);
        }

        /// <summary>겹친 만큼 점수(Overlap): Σ min(a[i], b[i])</summary>
        public static float Overlap(EmotionVector a, EmotionVector b)
        {
            return Mathf.Min(a.joy, b.joy)
                 + Mathf.Min(a.sadness, b.sadness)
                 + Mathf.Min(a.anger, b.anger)
                 + Mathf.Min(a.fear, b.fear)
                 + Mathf.Min(a.disgust, b.disgust)
                 + Mathf.Min(a.surprise, b.surprise)
                 + Mathf.Min(a.neutral, b.neutral);
        }
    }
}
