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

        [Header("Day Plans (optional)")]
        [SerializeField] private List<DayOrdersSO> dayPlans = new();

        [SerializeField] private bool verboseLog = true;
        [SerializeField] private string brewingSceneName = "BrewingScene";

        private int _servedCustomersToday;
        private CustomerOrdersAuthoring _activeCustomer;
        private List<OrderSlotRuntime> _slots;
        private int _slotIndex;

        private DayOrdersSO _todayPlan;
        private int _todayCustomerIndex;
        private int _customersTargetToday;
        private CustomerProfile _activeProfile;

        public void StartDay()
        {
            if (verboseLog) Debug.Log($"[Flow] StartDay day={dayCycle?.currentDay} customersPerDay={customersPerDay}");

            _servedCustomersToday = 0;
            _todayCustomerIndex = 0;

            dayCycle?.StartDay();

            int day = (dayCycle != null) ? dayCycle.currentDay : 1;
            _todayPlan = FindPlanForDay(day);
            _customersTargetToday = (_todayPlan != null) ? CountValidCustomers(_todayPlan) : customersPerDay;

            if (verboseLog)
                Debug.Log($"[Flow] DayPlan={(_todayPlan ? _todayPlan.name : "None")} targetCustomers={_customersTargetToday}");

            StartNextCustomer();
        }

        private void StartNextCustomer()
        {
            _slots = null;
            _slotIndex = 0;

            // DayPlan 우선
            if (_todayPlan != null && _todayPlan.customers != null)
            {
                // null 엔트리 스킵
                while (_todayCustomerIndex < _todayPlan.customers.Count && _todayPlan.customers[_todayCustomerIndex] == null)
                    _todayCustomerIndex++;

                if (_todayCustomerIndex < _todayPlan.customers.Count)
                {
                    var def = _todayPlan.customers[_todayCustomerIndex];
                    _activeProfile = def.profile;
                    _slots = def.BuildRuntime(orderSystem);
                    _activeCustomer = null; // (기존 필드) 폴백용이므로 비워둠

                    Debug.Log($"[Flow] DayPlan PickCustomer={_activeProfile.displayName} " +
                              $"idx={_todayCustomerIndex + 1}/{_todayPlan.customers.Count} slotsBuilt={_slots.Count} orderSystemNull={(orderSystem == null)}");
                }
            }

            // DayPlan이 없거나 실패하면 기존 랜덤 풀 사용
            if (_slots == null)
            {
                _activeCustomer = PickCustomer();
                if (_activeCustomer == null)
                {
                    Debug.LogWarning("[Flow] No customer in pool.");
                    return;
                }

                _activeProfile = _activeCustomer.profile;
                _slots = _activeCustomer.BuildRuntime(orderSystem);

                Debug.Log($"[Flow] Pool PickCustomer={_activeProfile.displayName} slotsBuilt={_slots.Count} orderSystemNull={(orderSystem == null)}");
            }

            if (_slots == null || _slots.Count == 0)
            {
                Debug.LogWarning("[Flow] Customer has 0 slots. Check slots / OrderSystem ref.");
                return;
            }

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
            bridge.BeginCustomer(_activeProfile);              // 네 브릿지 메서드명에 맞게
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
            _todayCustomerIndex++; // DayPlan 순번 진행

            if (_servedCustomersToday >= _customersTargetToday)
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

        private DayOrdersSO FindPlanForDay(int day)
        {
            if (dayPlans == null) return null;

            // (A) dayNumber로 매칭
            foreach (var p in dayPlans)
                if (p != null && p.dayNumber == day) return p;

            // (B) 리스트 인덱스로 폴백(1일차=0번)
            int idx = day - 1;
            if (idx >= 0 && idx < dayPlans.Count) return dayPlans[idx];

            return null;
        }

        private int CountValidCustomers(DayOrdersSO plan)
        {
            if (plan == null || plan.customers == null) return 0;
            int c = 0;
            foreach (var d in plan.customers)
                if (d != null) c++;
            return Mathf.Min(c, 3);
        }
    }
}
