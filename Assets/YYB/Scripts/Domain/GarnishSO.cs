using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alkuul.Domain
{
    [CreateAssetMenu(menuName = "Alkuul/Garnish")]
    public class GarnishSO : ScriptableObject
    {
        public string id, displayName;
        public string[] tags; // 감정 태그(주요 1차 감정과 매치용)
        public Sprite icon;
    }
}

