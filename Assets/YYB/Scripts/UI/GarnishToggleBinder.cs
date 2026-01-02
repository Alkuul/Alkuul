using UnityEngine;
using UnityEngine.UI;
using Alkuul.Domain;

namespace Alkuul.UI
{
    public class GarnishToggleBinder : MonoBehaviour
    {
        [SerializeField] private BrewingPanelBridge bridge;
        [SerializeField] private GarnishSO garnish;
        [SerializeField] private Toggle toggle;

        public void OnChanged(bool on)
        {
            if (bridge == null || garnish == null || toggle == null) return;

            bool ok = bridge.SetGarnishes(garnish, on);
            if (!ok)
                toggle.SetIsOnWithoutNotify(false);
        }
    }
}
