using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.UI
{
    public class TechniqueSelectButton : MonoBehaviour
    {
        [SerializeField] private BrewingPanelBridge bridge;
        [SerializeField] private TechniqueSO technique;

        public void Select() => bridge.SetTechnique(technique);
    }
}
