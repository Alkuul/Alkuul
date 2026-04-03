using System;
using System.Collections.Generic;
using UnityEngine;
using Alkuul.Domain.Brewing;

namespace Alkuul.UI.Brewing
{
    [Serializable]
    public class TechniqueInteractionEntry
    {
        public TechniqueType techniqueType;
        public TechniqueInteractionSpec spec;
        public TechniqueInteractionBase prefab;
    }

    public class TechniqueInteractionRegistry : MonoBehaviour
    {
        [SerializeField] private List<TechniqueInteractionEntry> entries = new();

        public bool TryGetEntry(TechniqueType type, out TechniqueInteractionEntry entry)
        {
            entry = entries.Find(e => e.techniqueType == type);
            return entry != null && entry.prefab != null && entry.spec != null;
        }
    }
}
