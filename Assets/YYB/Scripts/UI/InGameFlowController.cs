using Alkuul.Domain;
using Alkuul.Systems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
        [SerializeField] private OrderDialogueUI orderUI;
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
        private bool _dayPrepared;


        public void StartDay()
        {
            if (verboseLog) Debug.Log($"[Flow] StartDay day={dayCycle?.currentDay} customersPerDay={customersPerDay}");

            _servedCustomersToday = 0;
            _todayCustomerIndex = 0;

            _slots = null;
            _slotIndex = 0;
            _activeCustomer = null;
            _activeProfile = default;

            dayCycle?.StartDay();

            int day = (dayCycle != null) ? dayCycle.currentDay : 1;
            _todayPlan = FindPlanForDay(day);
            _customersTargetToday = (_todayPlan != null) ? CountValidCustomers(_todayPlan) : customersPerDay;

            _dayPrepared = true;

            if (verboseLog)
                Debug.Log($"[Flow] DayPlan={(_todayPlan ? _todayPlan.name : "None")} targetCustomers={_customersTargetToday}");

            Show(null);
        }

        public void ReceiveCustomer()
        {
            if (!_dayPrepared)
            {
                Debug.LogWarning("[Flow] Day not started. Call StartDay() first.");
                return;
            }

            if (_servedCustomersToday >= _customersTargetToday)
            {
                Debug.Log("[Flow] All customers served today.");
                Show(endDayPanel); // 원하면 정산/다음날 패널로
                return;
            }

            // 이미 손님 진행 중이면 중복 방지
            if (_slots != null && _slots.Count > 0 && _slotIndex < _slots.Count)
            {
                Debug.Log("[Flow] Customer already active.");
                return;
            }

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

            // 현재 손님 정보 비우기(태블릿에 이전 대사 남는 것 방지)
            _slots = null;
            _slotIndex = 0;
            _activeCustomer = null;
            _activeProfile = default;
            RefreshOrderPanelText(); // 있으면 비우는 용도

            if (_servedCustomersToday >= _customersTargetToday)
            {
                dayCycle?.EndDayPublic();
                Show(endDayPanel);
            }
            else
            {
                Show(null);
            }
        }

        public void OnClickNextDay()
        {
            StartDay();
        }

        private void RefreshOrderPanelText()
        {
            if (_slots == null || _slots.Count == 0) return;
            if (_slotIndex < 0 || _slotIndex >= _slots.Count) return;

            var slot = _slots[_slotIndex];

            string line = string.IsNullOrWhiteSpace(slot.dialogueLine)
                ? BuildAutoLine(slot.keywords)
                : slot.dialogueLine;

            if (orderUI != null)
                orderUI.Set(_activeProfile, _slotIndex + 1, _slots.Count, line, slot.order);
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

        public void BindOrderUI(OrderDialogueUI ui)
        {
            orderUI = ui;
            RefreshOrderPanelText(); // 이미 진행 중인 주문이 있으면 즉시 갱신
        }
        public bool TryGetCurrentOrderDialogue(out CustomerProfile profile, out int slotIndex1Based, out int slotCount, out string line)
        {
            profile = _activeProfile;
            slotIndex1Based = 0;
            slotCount = 0;
            line = "";

            if (_slots == null || _slots.Count == 0) return false;
            if (_slotIndex < 0 || _slotIndex >= _slots.Count) return false;

            var s = _slots[_slotIndex];
            slotCount = _slots.Count;
            slotIndex1Based = _slotIndex + 1;

            line = string.IsNullOrWhiteSpace(s.dialogueLine)
                ? BuildAutoLine(s.keywords)
                : s.dialogueLine;

            return true;
        }
    }
}
