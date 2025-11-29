using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alkuul.Domain
{
    [CreateAssetMenu(menuName = "Alkuul/Glass")]
    public class GlassSO : ScriptableObject
    {
        public string id, displayName;
        public string[] tags;     // ÀÏÄ¡ ½Ã +5%
        public int capacityMl;
        public Sprite icon;
    }
}

