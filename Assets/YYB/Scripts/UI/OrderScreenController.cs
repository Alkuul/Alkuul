using UnityEngine;

namespace Alkuul.UI
{
    public class OrderScreenController : MonoBehaviour
    {
        private InGameFlowController flow;

        private void Awake()
        {
            // CoreScene(또는 DontDestroyOnLoad) 쪽에 살아있는 Flow를 런타임에 찾음
            flow = FindObjectOfType<InGameFlowController>(true);

            if (flow == null)
                Debug.LogError("[OrderScreen] InGameFlowController not found. Core/GameRoot가 먼저 로드됐는지 확인.");
        }

        public void OnClickStartDay()
        {
            flow?.StartDay();
        }

        public void OnClickStartBrewing()
        {
            flow?.OnClickStartBrewing();
        }

        public void OnClickReceiveOrder()
        {
            flow?.OnClickReceiveOrder();
        }

        public void OnClickAdvanceDialogue()
        {
            flow?.AdvanceDialogue();
        }
    }
}
