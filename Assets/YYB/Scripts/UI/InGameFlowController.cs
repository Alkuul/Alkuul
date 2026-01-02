using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Alkuul.Domain;
using Alkuul.Systems;

namespace Alkuul.UI
{
    public class InGameFlowController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private DayCycleController dayCycle;
        [SerializeField] private OrderSystem orderSystem;
        [SerializeField] private BrewingPanelBridge bridge;

        [Header("Customers (scene authoring)")]
        [SerializeField] private List<CustomerOrdersAuthoring> customerPool = new();
        [SerializeField] private int customersPerDay = 3;

        [Header("Panels")]
        [SerializeField] private GameObject orderPanel;
        [SerializeField] private GameObject brewingPanel;
        [SerializeField] private GameObject endDayPanel;

        [Header("Order Panel UI (optional)")]
        [SerializeField] private Text orderText;  // TMP 쓰면 TextMeshProUGUI로 바꿔도 됨

        private int _servedCustomersToday;
        private CustomerOrdersAuthoring _activeCustomer;
        private List<OrderSlotRuntime> _slots;
        private int _slotIndex;

        public void StartDay()
        {
            _servedCustomersToday = 0;

            // DayCycleController가 1초 뒤 자동 종료하는 상태면, 아래 StartDay 호출을 쓰지 말고
            // 4-5 수정안을 먼저 적용해.
            dayCycle?.StartDay();

            StartNextCustomer();
        }

        private void StartNextCustomer()
        {
            _activeCustomer = PickCustomer();
            if (_activeCustomer == null)
            {
                Debug.LogWarning("[Flow] No customer in pool.");
                return;
            }

            _slots = _activeCustomer.BuildRuntime(orderSystem);
            _slotIndex = 0;

            bridge.BeginCustomer(_activeCustomer.profile);
            RefreshOrderPanelText();

            Show(orderPanel);
        }

        private CustomerOrdersAuthoring PickCustomer()
        {
            if (customerPool == null || customerPool.Count == 0) return null;
            return customerPool[Random.Range(0, customerPool.Count)];
        }

        public void OnClickStartBrewing()
        {
            if (_slots == null || _slotIndex >= _slots.Count)
            {
                Debug.LogWarning("[Flow] No slot to brew.");
                return;
            }

            bridge.SetCurrentOrder(_slots[_slotIndex].order);
            Show(brewingPanel);
        }

        public void OnClickServe()
        {
            // 1) 현재 주문 슬롯의 1잔 제출
            var r = bridge.ServeOnce();

            // 2) 떠났으면 즉시 손님 종료
            if (r.customerLeft)
            {
                FinishCustomerAndContinue();
                return;
            }

            // 3) 다음 슬롯로 진행(최대 3)
            _slotIndex++;
            if (_slotIndex >= _slots.Count)
            {
                FinishCustomerAndContinue();
            }
            else
            {
                RefreshOrderPanelText();
                Show(orderPanel);
            }
        }

        private void FinishCustomerAndContinue()
        {
            bridge.FinishCustomer();

            _servedCustomersToday++;
            if (_servedCustomersToday >= customersPerDay)
            {
                dayCycle?.EndDayPublic();
                Show(endDayPanel);
            }
            else
            {
                StartNextCustomer();
            }
        }

        public void OnClickNextDay()
        {
            StartDay();
        }

        private void RefreshOrderPanelText()
        {
            if (orderText == null) return;
            if (_slots == null || _slotIndex >= _slots.Count) { orderText.text = ""; return; }

            var s = _slots[_slotIndex];

            string line = string.IsNullOrWhiteSpace(s.dialogueLine)
                ? BuildAutoLine(s.keywords)
                : s.dialogueLine;

            orderText.text = $"주문 {_slotIndex + 1}/{_slots.Count}\n{line}";
        }

        private string BuildAutoLine(List<SecondaryEmotionSO> keywords)
        {
            if (keywords == null || keywords.Count == 0) return "(키워드 없음)";
            var names = keywords
                .Where(k => k != null)
                .Select(k => string.IsNullOrWhiteSpace(k.displayName) ? k.id : k.displayName);
            return string.Join(", ", names);
        }

        private void Show(GameObject panel)
        {
            if (orderPanel != null) orderPanel.SetActive(panel == orderPanel);
            if (brewingPanel != null) brewingPanel.SetActive(panel == brewingPanel);
            if (endDayPanel != null) endDayPanel.SetActive(panel == endDayPanel);
        }
    }
}
