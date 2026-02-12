using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alkuul.Domain
{
    [CreateAssetMenu(menuName = "Alkuul/Technique")]
    public class TechniqueSO : ScriptableObject
    {
        public string id, displayName;
        public string[] tags; // Shake=ºÐ³ë, Stir=½½ÇÄ µî

        [Header("Tooltip")]
        [TextArea(1, 2)] public string tooltipSummary;
        [TextArea(2, 5)] public string tooltipDetails;
    }
}

