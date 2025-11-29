using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alkuul.Domain
{
    [CreateAssetMenu(menuName = "Alkuul/Ingredient")]
    public class IngredientSO : ScriptableObject
    {
        public string id, displayName;
        public float abv;                // 도수(%)
        public EmotionVector emotions;   // 감정 기여(ml 비례)
        public Sprite icon;
    }
}

