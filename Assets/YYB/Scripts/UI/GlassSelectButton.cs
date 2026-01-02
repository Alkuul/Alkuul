using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.UI
{
    public class GlassSelectButton : MonoBehaviour
    {
        [SerializeField] private BrewingPanelBridge bridge;
        [SerializeField] private GlassSO Glass;

        public void Select() => bridge.SetGlass(Glass);
    }
}