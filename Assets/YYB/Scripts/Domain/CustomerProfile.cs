using UnityEngine;

namespace Alkuul.Domain
{
    public enum Tolerance
    {
        Weak,   // x1.25
        Normal, // x1.0
        Strong  // x0.75
    }

    public enum IcePreference
    {
        Neutral,
        Like,
        Dislike
    }

    [System.Serializable]
    public struct CustomerProfile
    {
        public string id;
        public string displayName;
        public Sprite portrait;
        public Tolerance tolerance;
        public IcePreference icePreference;
    }
}
