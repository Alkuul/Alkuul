using UnityEngine;

namespace Alkuul.Domain
{
    public enum Tolerance
    {
        Weak,   // ¡¿1.25
        Normal, // ¡¿1.0
        Strong  // ¡¿0.75
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
        public Tolerance tolerance;
        public IcePreference icePreference;
        public CustomerPortraitSet portraitSet;
    }
}

