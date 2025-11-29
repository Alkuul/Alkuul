using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.Systems
{
    public sealed class RepSystem : MonoBehaviour
    {
        [Range(0, 5)] public float reputation = 2.5f;

        public void Apply(CustomerResult cr)
        {
            reputation = Mathf.Clamp(reputation + cr.reputationDelta, 0f, 5f);
        }
    }
}
