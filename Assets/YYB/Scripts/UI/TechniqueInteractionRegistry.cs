using Alkuul.Domain;
using Alkuul.UI.Brewing;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TechniqueInteractionEntry
{
    public TechniqueSO techniqueData;
    public TechniqueInteractionSpec spec;
    public TechniqueInteractionBase prefab;
}

public class TechniqueInteractionRegistry : MonoBehaviour
{
    [SerializeField] private List<TechniqueInteractionEntry> entries = new();

    public bool TryGetEntry(TechniqueSO techniqueData, out TechniqueInteractionEntry entry)
    {
        entry = null;

        if (techniqueData == null)
            return false;

        foreach (var e in entries)
        {
            if (e == null) continue;
            if (e.techniqueData == null) continue;
            if (e.prefab == null) continue;
            if (e.spec == null) continue;

            if (e.techniqueData == techniqueData)
            {
                entry = e;
                return true;
            }
        }

        return false;
    }
}