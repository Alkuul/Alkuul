using Alkuul.Domain;
using Alkuul.Systems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        [SerializeField] private bool verboseLog = true;
        [SerializeField] private string brewingSceneName = "BrewingScene";

        private int _servedCustomersToday;
        private CustomerOrdersAuthoring _activeCustomer;
        private List<OrderSlotRuntime> _slots;
        private int _slotIndex;

        public void StartDay()
        {
            if (verboseLog) Debug.Log($"[Flow] StartDay day={dayCycle?.currentDay} customersPerDay={customersPerDay}");

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

            Debug.Log($"[Flow] PickCustomer={_activeCustomer.profile.displayName} slotsBuilt={_slots.Count} orderSystemNull={(orderSystem == null)}");

            if (_slots == null || _slots.Count == 0)
            {
                Debug.LogWarning("[Flow] Customer has 0 slots. Check CustomerOrdersAuthoring slots / OrderSystem ref.");
                return;
            }
        }

        private CustomerOrdersAuthoring PickCustomer()
        {
            if (customerPool == null || customerPool.Count == 0) return null;
            return customerPool[Random.Range(0, customerPool.Count)];
        }

        public void OnClickStartBrewing()
        {
            if (_slots == null || _slots.Count == 0)
            {
                Debug.LogWarning("[Flow] No slot to brew.");
                return;
            }

            if (_slotIndex < 0 || _slotIndex >= _slots.Count)
            {
                Debug.LogWarning("[Flow] Slot index out of range.");
                return;
            }

            // 1) 조주 씬 로드
            StartCoroutine(LoadBrewingAndBindBridge());
        }

        private IEnumerator LoadBrewingAndBindBridge()
        {
            // 씬 로드
            yield return SceneManager.LoadSceneAsync(brewingSceneName);

            // 브릿지 찾기(조주 씬에 있어야 함)
            bridge = FindObjectOfType<BrewingPanelBridge>();
            if (bridge == null)
            {
                Debug.LogError("[Flow] BrewingPanelBridge not found in BrewingScene.");
                yield break;
            }

            // 2) 현재 손님/현재 주문을 브릿지에 주입
            bridge.BeginCustomer(_activeCustomer.profile);              // 네 브릿지 메서드명에 맞게
            bridge.SetCurrentOrder(_slots[_slotIndex].order);           // 네 브릿지 메서드명에 맞게

            if (verboseLog)
                Debug.Log($"[Flow] Enter BrewingScene slot={_slotIndex + 1}/{_slots.Count}");
        }

        public void OnClickServe()
        {
            // 1) 현재 주문 슬롯의 1잔 제출
            var r = bridge.ServeOnce();

            if (verboseLog) Debug.Log($"[Flow] Serve result sat={r.satisfaction} left={r.customerLeft}");

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
