using Alkuul.Domain;
using Alkuul.Systems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alkuul.UI
{
    public class InGameFlowController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private DayCycleController dayCycle;
        [SerializeField] private OrderSystem orderSystem;

        [Header("Scene Names")]
        [SerializeField] private string orderSceneName = "OrderScene";
        [SerializeField] private string brewingSceneName = "BrewingScene";

        [Header("Customers (Authoring)")]
        [SerializeField] private List<CustomerOrdersAuthoring> customerPool = new();
        [SerializeField] private int customersPerDay = 3;

        [Header("Day Plans (optional)")]
        [SerializeField] private List<DayOrdersSO> dayPlans = new();

        [Header("Order Gate")]
        [SerializeField] private bool requireReceiveOrderButton = true;
        [SerializeField] private string promptBeforeStartDay = "태블릿을 열어 하루를 시작하세요.";
        [SerializeField] private string promptBeforeReceiveCustomer = "손님을 맞이하세요.";
        [SerializeField] private string promptBeforeReceiveOrder = "주문받기를 눌러 주문을 확인하세요.";
        [SerializeField] private string promptBeforeSettlement = "정산하기를 눌러 하루를 마무리하세요.";
        [SerializeField] private string promptDuringRename = "술 이름을 정해주세요.";
        [SerializeField] private string promptDuringInnDecision = "손님이 만취 상태입니다. 여관에 재우시겠습니까?";

        [Header("UI (bound by OrderSceneBinder)")]
        [SerializeField] private OrderDialogueUI orderUI;
        [SerializeField] private CustomerPortraitView portraitView;

        [Header("Inn Decision")]
        [SerializeField] private PendingInnDecisionSystem innDecision;

        [Header("Debug")]
        [SerializeField] private bool verboseLog = true;

        // runtime
        private DayOrdersSO _todayPlan;
        private int _todayCustomerIndex;
        private int _customersTargetToday;
        private int _servedCustomersToday;

        private CustomerProfile _activeProfile;
        private List<OrderSlotRuntime> _slots;
        private int _slotIndex;

        private bool _dayPrepared;
        private bool _awaitingReceiveCustomer;
        private bool _awaitingReceiveOrder;
        private bool _awaitingSettlement;
        private bool _awaitingRename;
        private bool _awaitingDayIntro;
        private bool _awaitingPostServeDialogue;

        private int _dayIntroIndex;
        private int _postServeIndex;
        private List<string> _dayIntroLines = new();
        private List<string> _postServeLines = new();
        private bool _innDecisionBound;

        // rename pending
        private BrewingPanelBridge _bridge;
        private Drink _pendingDrink;
        private DrinkResult _pendingDrinkResult;
        private PendingAdvance _pendingAdvance = PendingAdvance.None;

        private readonly List<Drink> _currentCustomerDrinks = new();
        private readonly List<DrinkResult> _currentCustomerResults = new();
        private bool _currentCustomerLeftEarly;

        private enum PendingAdvance
        {
            None,
            NextSlot,
            NextCustomer,
            EndDay
        }

        public bool DayPrepared => _dayPrepared;
        public bool AwaitingReceiveCustomer => _awaitingReceiveCustomer;
        public bool AwaitingReceiveOrder => _awaitingReceiveOrder;
        public bool AwaitingSettlement => _awaitingSettlement;
        public bool AwaitingRename => _awaitingRename;

        private void OnEnable()
        {
            EnsureRefs();
        }

        private void OnDisable()
        {
            if (innDecision != null)
                innDecision.QueueChanged -= HandleInnDecisionQueueChanged;
            _innDecisionBound = false;
        }


        public void RefreshOrderUIExternal()
        {
            RefreshOrderUI();
        }

        // ---- bindings ----
        public void BindOrderUI(OrderDialogueUI ui)
        {
            orderUI = ui;
            EnsureRefs();
            UpdatePortraitForActiveCustomer();
            RefreshOrderUI();
        }

        public void AdvanceDialogue()
        {
            if (_awaitingDayIntro)
            {
                _dayIntroIndex++;
                if (_dayIntroIndex >= _dayIntroLines.Count)
                {
                    _awaitingDayIntro = false;
                    _dayIntroIndex = 0;
                    _awaitingReceiveCustomer = true;
                }

                RefreshOrderUI();
                return;
            }

            if (_awaitingPostServeDialogue)
            {
                _postServeIndex++;
                if (_postServeIndex >= _postServeLines.Count)
                {
                    _awaitingPostServeDialogue = false;
                    _postServeIndex = 0;
                    ApplyPendingAdvance();
                }

                RefreshOrderUI();
            }
        }

        // ---- day flow ----
        public void StartDay()
        {
            EnsureRefs();

            if (_awaitingRename)
            {
                Debug.LogWarning("[Flow] StartDay blocked: awaiting rename.");
                return;
            }

            _dayPrepared = true;
            _awaitingSettlement = false;

            _servedCustomersToday = 0;
            _todayCustomerIndex = 0;

            _slots = null;
            _slotIndex = 0;
            _activeProfile = default;

            dayCycle?.StartDay();

            int dayNum = dayCycle != null ? dayCycle.currentDay : 1;
            _todayPlan = FindPlanForDay(dayNum);
            _customersTargetToday = (_todayPlan != null) ? CountValidCustomers(_todayPlan) : customersPerDay;

            _dayIntroLines = FilterDialogueLines(_todayPlan != null ? _todayPlan.dayIntroLines : null);
            _dayIntroIndex = 0;
            _awaitingDayIntro = _dayIntroLines.Count > 0;

            _awaitingReceiveCustomer = !_awaitingDayIntro;
            _awaitingReceiveOrder = false;
            _awaitingPostServeDialogue = false;
            _postServeIndex = 0;
            _postServeLines.Clear();

            if (verboseLog)
                Debug.Log($"[Flow] DayPrepared day={dayNum} plan={(_todayPlan ? _todayPlan.name : "None")} targetCustomers={_customersTargetToday}");

            RefreshOrderUI();
        }

        /// <summary>태블릿의 "손님받기" 버튼에서 호출</summary>
        public void ReceiveCustomer()
        {
            EnsureRefs();

            if (!_dayPrepared)
            {
                Debug.LogWarning("[Flow] ReceiveCustomer blocked: day not started.");
                RefreshOrderUI();
                return;
            }

            if (_awaitingSettlement)
            {
                Debug.LogWarning("[Flow] ReceiveCustomer blocked: awaiting settlement.");
                RefreshOrderUI();
                return;
            }

            if (_awaitingDayIntro)
            {
                RefreshOrderUI();
                return;
            }

            if (innDecision != null && innDecision.HasPending)
            {
                Debug.LogWarning("[Flow] ReceiveCustomer blocked: pending inn decision.");
                RefreshOrderUI();
                return;
            }

            if (_awaitingRename)
            {
                Debug.LogWarning("[Flow] ReceiveCustomer blocked: awaiting rename.");
                return;
            }

            if (!_awaitingReceiveCustomer)
            {
                // 이미 손님이 있는 상태
                RefreshOrderUI();
                return;
            }

            if (!StartNextCustomerInternal())
            {
                // 손님이 없으면 정산 상태로
                _awaitingSettlement = true;
                _awaitingReceiveCustomer = false;
                RefreshOrderUI();
                return;
            }

            _awaitingReceiveCustomer = false;
            EnterAwaitingOrderState();
            RefreshOrderUI();
        }

        /// <summary>태블릿의 "주문받기" 버튼에서 호출</summary>
        public void OnClickReceiveOrder()
        {
            if (_awaitingRename || _awaitingSettlement) return;
            if (!_dayPrepared) { RefreshOrderUI(); return; }
            if (_awaitingReceiveCustomer) { RefreshOrderUI(); return; }

            if (requireReceiveOrderButton && _awaitingReceiveOrder)
            {
                _awaitingReceiveOrder = false;
                RefreshOrderUI();
            }
        }

        /// <summary>오더씬의 "조주하러가기" 버튼(또는 UI)에서 호출</summary>
        public void OnClickStartBrewing()
        {
            EnsureRefs();

            if (!_dayPrepared)
            {
                Debug.LogWarning("[Flow] StartBrewing blocked: day not started.");
                RefreshOrderUI();
                return;
            }
            if (_awaitingSettlement)
            {
                Debug.LogWarning("[Flow] StartBrewing blocked: awaiting settlement.");
                RefreshOrderUI();
                return;
            }
            if (_awaitingRename)
            {
                Debug.LogWarning("[Flow] StartBrewing blocked: awaiting rename.");
                return;
            }
            if (_awaitingReceiveCustomer)
            {
                Debug.LogWarning("[Flow] StartBrewing blocked: no active customer. Press ReceiveCustomer first.");
                RefreshOrderUI();
                return;
            }
            if (!EnsureHasCurrentSlot()) return;

            if (requireReceiveOrderButton && _awaitingReceiveOrder)
            {
                Debug.LogWarning("[Flow] StartBrewing blocked: press ReceiveOrder first.");
                RefreshOrderUI();
                return;
            }

            StartCoroutine(LoadBrewingAndBindBridge());
        }

        /// <summary>브루잉씬의 "제출" 버튼에서 호출 (BrewingScreenController가 호출)</summary>
        public void OnClickServe()
        {
            if (_awaitingRename) return;

            _bridge = FindObjectOfType<BrewingPanelBridge>(true);
            if (_bridge == null)
            {
                Debug.LogWarning("[Flow] OnClickServe: BrewingPanelBridge not found.");
                return;
            }

            // 1) 한 잔 제공
            var r = _bridge.ServeOnce();
            bool hasServed = _bridge.TryGetLastServed(out _pendingDrink, out _pendingDrinkResult);

            if (hasServed)
            {
                _currentCustomerDrinks.Add(_pendingDrink);
                _currentCustomerResults.Add(_pendingDrinkResult);
            }
            if (r.customerLeft) _currentCustomerLeftEarly = true;

            if (verboseLog) Debug.Log($"[Flow] ServeOnce sat={r.satisfaction} left={r.customerLeft}");

            // 2) 이 잔 제공 후, 손님 종료 여부 판단(도망 or 마지막 잔)
            bool isLastSlot = (_slots == null) || (_slotIndex >= (_slots.Count - 1));
            bool customerEnded = r.customerLeft || isLastSlot;

            // 3) 손님 종료면(마지막 잔 포함) CustomerResult 계산/반영은 지금 바로 처리(브루잉씬에 bridge가 있을 때)
            if (customerEnded)
            {
                _bridge.FinishCustomer();
            }

            // 4) 다음 진행은 "이름정하기 확정" 이후에만!
            if (!customerEnded)
            {
                _pendingAdvance = PendingAdvance.NextSlot;
            }
            else
            {
                // 손님 종료 후, 오늘 목표 손님 수 달성 여부
                int servedAfterThis = _servedCustomersToday + 1;
                _pendingAdvance = (servedAfterThis >= _customersTargetToday) ? PendingAdvance.EndDay : PendingAdvance.NextCustomer;
            }

            _awaitingRename = true;
            RefreshOrderUI();

            // 5) 오더씬으로 돌아가서 Rename 패널 열기
            StartCoroutine(ReturnToOrderAndOpenRename());
        }

        /// <summary>Rename 패널 "확인" 버튼에서 호출 (TabletController.ConfirmRename → 여기로 전달)</summary>
        public void ConfirmRenameAndContinue(string drinkName)
        {
            if (!_awaitingRename) return;

            _awaitingRename = false;

            string finalName = string.IsNullOrWhiteSpace(drinkName)
                ? BuildDefaultDrinkName()
                : drinkName.Trim();

            if (verboseLog) Debug.Log($"[Flow] Rename confirmed: {finalName} advance={_pendingAdvance}");

            PreparePostServeDialogue();
            if (_awaitingPostServeDialogue)
            {
                RefreshOrderUI();
                return;
            }

            ApplyPendingAdvance();

            RefreshOrderUI();
        }

        /// <summary>태블릿의 "정산하기" 버튼에서 호출 (하루 종료 이벤트 발생)</summary>
        public void OnClickSettlement()
        {
            EnsureRefs();

            if (!_awaitingSettlement)
            {
                Debug.LogWarning("[Flow] Settlement blocked: not awaiting settlement.");
                RefreshOrderUI();
                return;
            }

            dayCycle?.EndDayPublic();

            // 다음 날은 "StartDay"를 눌러야 시작
            _dayPrepared = false;
            _awaitingSettlement = false;
            _awaitingReceiveCustomer = false;
            _awaitingReceiveOrder = false;
            _awaitingRename = false;
            _awaitingDayIntro = false;
            _awaitingPostServeDialogue = false;
            _dayIntroIndex = 0;
            _postServeIndex = 0;
            _dayIntroLines.Clear();
            _postServeLines.Clear();

            ClearActiveCustomer();
            RefreshOrderUI();
        }

        // ---- helpers ----
        private void EnsureRefs()
        {
            if (dayCycle == null) dayCycle = FindObjectOfType<DayCycleController>(true);
            if (orderSystem == null) orderSystem = FindObjectOfType<OrderSystem>(true);
            if (portraitView == null) portraitView = FindObjectOfType<CustomerPortraitView>(true);
            if (innDecision == null) innDecision = FindObjectOfType<PendingInnDecisionSystem>(true);
            BindInnDecision();
        }

        private void BindInnDecision()
        {
            if (_innDecisionBound || innDecision == null) return;
            innDecision.QueueChanged += HandleInnDecisionQueueChanged;
            _innDecisionBound = true;
        }

        private void HandleInnDecisionQueueChanged()
        {
            RefreshOrderUI();
        }

        private IEnumerator LoadBrewingAndBindBridge()
        {
            yield return SceneManager.LoadSceneAsync(brewingSceneName);

            _bridge = FindObjectOfType<BrewingPanelBridge>(true);
            if (_bridge == null)
            {
                Debug.LogError("[Flow] BrewingPanelBridge not found in BrewingScene.");
                yield break;
            }

            _bridge.BeginCustomer(_activeProfile);
            if (_currentCustomerDrinks.Count > 0 || _currentCustomerResults.Count > 0 || _currentCustomerLeftEarly)
                _bridge.RestoreSession(_currentCustomerDrinks, _currentCustomerResults, _currentCustomerLeftEarly);
            _bridge.SetCurrentOrder(_slots[_slotIndex].order);

            if (verboseLog)
                Debug.Log($"[Flow] Enter BrewingScene slot={_slotIndex + 1}/{_slots.Count}");
        }

        private IEnumerator ReturnToOrderAndOpenRename()
        {
            yield return SceneManager.LoadSceneAsync(orderSceneName);
            EnsureRefs();
            UpdatePortraitForActiveCustomer();

            TryOpenRenameUI();
            RefreshOrderUI();
        }

        private bool StartNextCustomerInternal()
        {
            if (orderSystem == null)
            {
                Debug.LogWarning("[Flow] OrderSystem missing.");
                return false;
            }

            _slots = null;
            _slotIndex = 0;

            // 1) DayPlan 우선
            if (_todayPlan != null && _todayPlan.customers != null)
            {
                while (_todayCustomerIndex < _todayPlan.customers.Count && _todayPlan.customers[_todayCustomerIndex] == null)
                    _todayCustomerIndex++;

                if (_todayCustomerIndex < _todayPlan.customers.Count)
                {
                    var def = _todayPlan.customers[_todayCustomerIndex];
                    _activeProfile = def.profile;
                    _slots = def.BuildRuntime(orderSystem);
                    ResetCustomerSession();

                    if (verboseLog)
                        Debug.Log($"[Flow] DayPlan PickCustomer={_activeProfile.displayName} idx={_todayCustomerIndex + 1}/{_todayPlan.customers.Count} slotsBuilt={_slots.Count}");
                }
            }

            // 2) Plan 없으면 pool 랜덤
            if (_slots == null)
            {
                var picked = PickCustomer();
                if (picked == null)
                {
                    Debug.LogWarning("[Flow] No customer in pool.");
                    return false;
                }

                _activeProfile = picked.profile;
                _slots = picked.BuildRuntime(orderSystem);
                ResetCustomerSession();

                if (verboseLog)
                    Debug.Log($"[Flow] Pool PickCustomer={_activeProfile.displayName} slotsBuilt={_slots.Count}");
            }

            if (_slots == null || _slots.Count == 0)
            {
                Debug.LogWarning("[Flow] Customer has 0 slots. Check slots / OrderSystem ref.");
                return false;
            }

            UpdatePortraitForActiveCustomer();
            return true;
        }

        private CustomerOrdersAuthoring PickCustomer()
        {
            if (customerPool == null || customerPool.Count == 0) return null;
            return customerPool[Random.Range(0, customerPool.Count)];
        }

        private void ClearActiveCustomer()
        {
            _slots = null;
            _slotIndex = 0;
            _activeProfile = default;
            ResetCustomerSession();
            if (portraitView != null) portraitView.Clear();
        }

        private void ResetCustomerSession()
        {
            _currentCustomerDrinks.Clear();
            _currentCustomerResults.Clear();
            _currentCustomerLeftEarly = false;
        }

        private void UpdatePortraitForActiveCustomer()
        {
            if (portraitView == null) return;

            if (string.IsNullOrEmpty(_activeProfile.id))
            {
                portraitView.Clear();
                return;
            }

            if (_activeProfile.portraitSet == null)
            {
                portraitView.Clear();
                if (verboseLog) Debug.LogWarning("[Flow] Active customer has no portrait set.");
                return;
            }

            var stage = GetPortraitStageForCurrentCustomer();
            portraitView.Bind(_activeProfile.portraitSet, stage);
        }

        private IntoxStage GetPortraitStageForCurrentCustomer()
        {
            if (string.IsNullOrEmpty(_activeProfile.id)) return IntoxStage.Sober;

            int points = IntoxSystem.ComputePoints(_currentCustomerDrinks, _activeProfile.tolerance);
            int stage = IntoxSystem.GetStage(points);

            if (stage <= 1) return IntoxStage.Sober;
            if (stage == 2) return IntoxStage.Tipsy;
            if (stage == 3) return IntoxStage.Drunk;
            return IntoxStage.Wasted;
        }

        private bool TryGetPendingInnDecisionProfile(out CustomerProfile profile, out IntoxStage stage)
        {
            profile = default;
            stage = IntoxStage.Sober;

            if (innDecision == null) return false;
            if (!innDecision.TryPeek(out var result)) return false;

            profile = FindCustomerProfileById(result.customerId);
            stage = GetPortraitStageFromIntoxStageValue(result.intoxStage);
            return true;
        }

        private CustomerProfile FindCustomerProfileById(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || customerPool == null) return default;

            if (!string.IsNullOrWhiteSpace(_activeProfile.id) && _activeProfile.id == id)
                return _activeProfile;

            if (_todayPlan != null && _todayPlan.customers != null)
            {
                foreach (var def in _todayPlan.customers)
                {
                    if (def == null) continue;
                    var profile = def.profile;
                    if (profile.id == id) return profile;
                }
            }

            foreach (var customer in customerPool)
            {
                if (customer == null) continue;
                var profile = customer.profile;
                if (profile.id == id) return profile;
            }

            return default;
        }

        private IntoxStage GetPortraitStageFromIntoxStageValue(int stage)
        {
            if (stage <= 1) return IntoxStage.Sober;
            if (stage == 2) return IntoxStage.Tipsy;
            if (stage == 3) return IntoxStage.Drunk;
            return IntoxStage.Wasted;
        }


        private void EnterAwaitingOrderState()
        {
            _awaitingReceiveOrder = requireReceiveOrderButton;
        }

        private void PreparePostServeDialogue()
        {
            _postServeLines = FilterDialogueLines(GetPostServeLinesForCurrentSlot());
            _postServeIndex = 0;
            _awaitingPostServeDialogue = _postServeLines.Count > 0;
        }

        private void ApplyPendingAdvance()
        {
            
            switch (_pendingAdvance)
            {
                case PendingAdvance.NextSlot:
                    _slotIndex++;
                    EnterAwaitingOrderState(); 
                    break;

                case PendingAdvance.NextCustomer:
                    _servedCustomersToday++;
                    _todayCustomerIndex++;
                    ClearActiveCustomer();
                    _awaitingReceiveCustomer = true;
                    break;

                case PendingAdvance.EndDay:
                    _servedCustomersToday++;
                    _todayCustomerIndex++;
                    ClearActiveCustomer();
                    _awaitingSettlement = true;
                    _awaitingReceiveCustomer = false;
                    break;
            }

            _pendingAdvance = PendingAdvance.None;
        }

        private List<string> GetPostServeLinesForCurrentSlot()
        {
            if (_slots == null || _slotIndex < 0 || _slotIndex >= _slots.Count) return null;
            return _slots[_slotIndex].postServeLines;
        }

        private void TryOpenRenameUI()
        {
            if (!_awaitingRename) return;

            var tablet = FindObjectOfType<TabletController>(true);
            if (tablet != null)
            {
                int slotCount = _slots != null ? _slots.Count : 0;
                tablet.OpenRename(_pendingDrink, _pendingDrinkResult, _activeProfile, _slotIndex + 1, slotCount);
            }
            else
            {
                Debug.LogWarning("[Flow] TabletController not found. Rename UI won't open.");
            }
        }

        private string GetDayIntroLine()
        {
            if (_dayIntroLines == null || _dayIntroLines.Count == 0) return "";
            if (_dayIntroIndex < 0 || _dayIntroIndex >= _dayIntroLines.Count) return "";
            return _dayIntroLines[_dayIntroIndex];
        }

        private string GetPostServeLine()
        {
            if (_postServeLines == null || _postServeLines.Count == 0) return "";
            if (_postServeIndex < 0 || _postServeIndex >= _postServeLines.Count) return "";
            return _postServeLines[_postServeIndex];
        }

        private List<string> FilterDialogueLines(List<string> lines)
        {
            if (lines == null || lines.Count == 0) return new List<string>();
            return lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()).ToList();
        }

        private bool EnsureHasCurrentSlot()
        {
            if (_slots == null || _slots.Count == 0)
            {
                Debug.LogWarning("[Flow] No slots.");
                return false;
            }
            if (_slotIndex < 0 || _slotIndex >= _slots.Count)
            {
                Debug.LogWarning("[Flow] Slot index out of range.");
                return false;
            }
            return true;
        }

        public bool TryGetCurrentOrderDialogue(out CustomerProfile profile, out int idx1, out int cnt, out string line, out bool showMeta)
        {
            profile = default;
            idx1 = 0;
            cnt = 0;
            line = "";
            showMeta = true;

            if (!_dayPrepared)
            {
                line = promptBeforeStartDay;
                return true;
            }

            if (_awaitingSettlement)
            {
                line = promptBeforeSettlement;
                return true;
            }

            if (innDecision != null && innDecision.HasPending)
            {
                line = promptDuringInnDecision;
                return true;
            }

            if (_awaitingDayIntro)
            {
                line = GetDayIntroLine();
                showMeta = false;
                return true;
            }

            if (_awaitingPostServeDialogue)
            {
                profile = _activeProfile;
                idx1 = _slotIndex + 1;
                cnt = _slots != null ? _slots.Count : 0;
                line = GetPostServeLine();
                showMeta = false;
                return true;
            }

            if (_awaitingReceiveCustomer)
            {
                line = promptBeforeReceiveCustomer;
                return true;
            }

            if (_awaitingRename)
            {
                profile = _activeProfile;
                line = promptDuringRename;
                showMeta = false;
                return true;
            }

            if (!EnsureHasCurrentSlot())
            {
                line = "(주문 없음)";
                return false;
            }

            profile = _activeProfile;
            idx1 = _slotIndex + 1;
            cnt = _slots.Count;

            if (requireReceiveOrderButton && _awaitingReceiveOrder)
            {
                line = promptBeforeReceiveOrder;
                return true;
            }

            var slot = _slots[_slotIndex];
            line = string.IsNullOrWhiteSpace(slot.dialogueLine)
                ? BuildAutoLine(slot.keywords)
                : slot.dialogueLine;

            return true;
        }

        private string BuildAutoLine(List<SecondaryEmotionSO> keywords)
        {
            if (keywords == null || keywords.Count == 0) return "(키워드 없음)";
            var names = keywords
                .Where(k => k != null)
                .Select(k => string.IsNullOrWhiteSpace(k.displayName) ? k.id : k.displayName);
            return string.Join(", ", names);
        }

        private string BuildDefaultDrinkName()
        {
            // 간단 기본 이름
            string cname = string.IsNullOrWhiteSpace(_activeProfile.displayName) ? _activeProfile.id : _activeProfile.displayName;
            return $"{cname}의 술";
        }

        private void RefreshOrderUI()
        {
            if (orderUI == null) return;

            if (innDecision != null && innDecision.HasPending)
            {
                if (TryGetPendingInnDecisionProfile(out var pendingProfile, out var pendingStage)
                    && pendingProfile.portraitSet != null)
                {
                    portraitView?.Bind(pendingProfile.portraitSet, pendingStage);
                }
                else
                {
                    portraitView?.Clear();
                }
            }
            else
            {
                UpdatePortraitForActiveCustomer();
            }

            if (TryGetCurrentOrderDialogue(out var p, out var idx, out var cnt, out var line, out var showMeta))
            {
                if (cnt <= 0)
                {
                    orderUI.SetSystemLine(line);
                    return;
                }

                // 주문 메타는 슬롯 있을 때만 표시
                var order = _slots[idx - 1].order;
                bool allowMeta = showMeta && !(requireReceiveOrderButton && _awaitingReceiveOrder);
                orderUI.Set(p, idx, cnt, line, order, allowMeta);
            }
        }

        private DayOrdersSO FindPlanForDay(int day)
        {
            if (dayPlans == null) return null;

            // (A) dayNumber match
            foreach (var p in dayPlans)
                if (p != null && p.dayNumber == day) return p;

            // (B) list index fallback
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
