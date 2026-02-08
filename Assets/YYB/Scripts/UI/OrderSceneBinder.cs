using UnityEngine;

namespace Alkuul.UI
{
    public class OrderSceneBinder : MonoBehaviour
    {
        [SerializeField] private OrderDialogueUI orderDialogueUI;

        private void Awake()
        {
            var flow = FindObjectOfType<InGameFlowController>(true);
            if (flow == null)
            {
                Debug.LogError("[OrderSceneBinder] Flow not found. GameRoot/Flow가 먼저 떠야 함.");
                return;
            }

            flow.BindOrderUI(orderDialogueUI);
        }
    }
}
