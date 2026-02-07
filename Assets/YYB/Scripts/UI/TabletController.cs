using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Alkuul.Domain;
using Alkuul.Systems;
using Alkuul.UI;

public class TabletController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject tabletRoot;

    [Header("Pages")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;

    [Header("Page1 Panels")]
    [SerializeField] private GameObject p1_home;        // Panel1: 손님받기(홈)
    [SerializeField] private GameObject p1_rename;      // Panel2: 이름 정하기
    [SerializeField] private GameObject p1_innDecision; // Panel3: 재우기/쫓아내기
    [SerializeField] private GameObject p1_receiveOrder; // Panel4: 주문받기

    [Header("Page2 Panels")]
    [SerializeField] private GameObject p2_settleButton;  // Panel1: 정산하기 버튼
    [SerializeField] private GameObject p2_settleResult;  // Panel2: 정산 결과 + 다음 화면 버튼
    [SerializeField] private GameObject p2_startDay;      // Panel3: 하루 시작 버튼
    [SerializeField] private GameObject p2_upgrade12;     // Panel4: 여관 1->2
    [SerializeField] private GameObject p2_upgrade23;     // Panel5: 여관 2->3

    [Header("Header TMP (optional)")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text repText;
    [SerializeField] private TMP_Text innText;

    [Header("Order Dialogue TMP on Tablet (optional)")]
    [SerializeField] private TMP_Text tabletCustomerNameText;
    [SerializeField] private TMP_Text tabletDialogueText;

    [Header("Rename TMP (optional)")]
    [SerializeField] private TMP_Text renameInfoText;
    [SerializeField] private TMP_InputField renameInput;

    [Header("Settlement TMP (optional)")]
    [SerializeField] private TMP_Text settlementText;

    [Header("Upgrade TMP (optional)")]
    [SerializeField] private TMP_Text upgradeTitleText;
    [SerializeField] private TMP_Text upgradeCostText;

    [Header("Behavior")]
    [SerializeField] private bool autoSyncWhenOpened = true;  // 열릴 때 Flow 상태로 자동 이동
    [SerializeField] private bool autoSyncWhileOpen = true;   // 열려있는 동안에도 상태 변화 반영

    // Systems/Flow
    [SerializeField] private InGameFlowController flow;
    [SerializeField] private DayCycleController day;
    [SerializeField] private EconomySystem economy;
    [SerializeField] private RepSystem rep;
    [SerializeField] private InnUpgradeSystem innUpgrade;
    [SerializeField] private DailyLedgerSystem ledger;
    [SerializeField] private PendingInnDecisionSystem innDecision;

    // internal
    private GameObject _lastShownPanel;
    private bool _settlementDoneView = false;

    private bool _lockToSettlementResult = false;

    public bool IsOpen => tabletRoot != null && tabletRoot.activeSelf;

    private void Awake()
    {
        ResolveRefs();

        // 시작은 닫아두고, 마지막 패널은 홈
        _lastShownPanel = p1_home;
        SetOpen(false);
        ShowPanel(p1_home);
        RefreshHeader();
        RefreshTabletDialogue();
    }

    private void Update()
    {
        RefreshHeader();

        if (IsOpen && autoSyncWhileOpen)
        {
            SyncPanelToState();
            RefreshTabletDialogue();
        }
    }

    private void ResolveRefs()
    {
        if (flow == null) flow = FindObjectOfType<InGameFlowController>(true);
        if (day == null) day = FindObjectOfType<DayCycleController>(true);
        if (economy == null) economy = FindObjectOfType<EconomySystem>(true);
        if (rep == null) rep = FindObjectOfType<RepSystem>(true);
        if (innUpgrade == null) innUpgrade = FindObjectOfType<InnUpgradeSystem>(true);
        if (ledger == null) ledger = FindObjectOfType<DailyLedgerSystem>(true);
        if (innDecision == null) innDecision = FindObjectOfType<PendingInnDecisionSystem>(true);
    }

    // -------------------------
    // Open / Close
    // -------------------------
    public void ToggleOpen()
    {
        SetOpen(!IsOpen);
    }

    public void SetOpen(bool open)
    {
        if (tabletRoot) tabletRoot.SetActive(open);

        if (open)
        {
            ResolveRefs();

            if (autoSyncWhenOpened)
                SyncPanelToState(true);
            else if (_lastShownPanel != null)
                ShowPanel(_lastShownPanel);

            RefreshTabletDialogue();
        }
    }

    public void OnClick_Close()
    {
        SetOpen(false);
    }

    // -------------------------
    // Panel Show Helpers
    // -------------------------
    private void HideAll()
    {
        if (page1) page1.SetActive(false);
        if (page2) page2.SetActive(false);

        if (p1_home) p1_home.SetActive(false);
        if (p1_rename) p1_rename.SetActive(false);
        if (p1_innDecision) p1_innDecision.SetActive(false);
        if (p1_receiveOrder) p1_receiveOrder.SetActive(false);

        if (p2_settleButton) p2_settleButton.SetActive(false);
        if (p2_settleResult) p2_settleResult.SetActive(false);
        if (p2_startDay) p2_startDay.SetActive(false);
        if (p2_upgrade12) p2_upgrade12.SetActive(false);
        if (p2_upgrade23) p2_upgrade23.SetActive(false);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == null) return;

        HideAll();

        // 패널이 Page1 소속인지 Page2 소속인지 판단해서 페이지 먼저 켬
        bool isPage1 =
            panel == p1_home || panel == p1_rename || panel == p1_innDecision || panel == p1_receiveOrder;

        if (page1) page1.SetActive(isPage1);
        if (page2) page2.SetActive(!isPage1);

        panel.SetActive(true);
        _lastShownPanel = panel;
    }

    // -------------------------
    // Auto Sync: Flow 상태에 맞게 패널 전환
    // -------------------------
    private void SyncPanelToState(bool force = false)
    {
        if (flow == null) return;

        if (_lockToSettlementResult)
        {
            if (_lastShownPanel != p2_settleResult) ShowPanel(p2_settleResult);
            RefreshSettlementText();
            return;
        }

        // 업그레이드 패널을 사용자가 수동으로 열어둔 상태는 유지하고 싶으면 force=false일 때는 건드리지 않기
        if (!force && (_lastShownPanel == p2_upgrade12 || _lastShownPanel == p2_upgrade23))
            return;

        // 우선순위:
        // 1) Rename 중이면 Rename 패널
        if (flow.AwaitingRename)
        {
            if (_lastShownPanel != p1_rename) ShowPanel(p1_rename);
            return;
        }

        // 2) 숙박/퇴실 결정 대기(큐가 있으면) -> p1_innDecision
        if (innDecision != null && innDecision.HasPending)
        {
            if (_lastShownPanel != p1_innDecision) ShowPanel(p1_innDecision);
            return;
        }

        // 3) 정산 대기 상태
        if (flow.AwaitingSettlement)
        {
            // 정산을 아직 안 눌렀으면 정산 버튼 패널
            if (!_settlementDoneView)
            {
                if (_lastShownPanel != p2_settleButton) ShowPanel(p2_settleButton);
            }
            else
            {
                // 정산 누른 뒤라면 결과 패널
                if (_lastShownPanel != p2_settleResult) ShowPanel(p2_settleResult);
                RefreshSettlementText();
            }
            return;
        }

        // 4) 하루 준비 안 됨(= StartDay 누르기 전) -> 하루 시작 패널
        if (!flow.DayPrepared)
        {
            if (_lastShownPanel != p2_startDay) ShowPanel(p2_startDay);
            return;
        }

        // 5) 손님 받기 대기 -> 홈(손님받기)
        if (flow.AwaitingReceiveCustomer)
        {
            if (_lastShownPanel != p1_home) ShowPanel(p1_home);
            return;
        }

        // 6) 주문받기 대기 -> 주문받기 패널
        if (flow.AwaitingReceiveOrder)
        {
            if (_lastShownPanel != p1_receiveOrder) ShowPanel(p1_receiveOrder);
            return;
        }

        // 기본: 홈
        if (_lastShownPanel != p1_home) ShowPanel(p1_home);
    }

    // -------------------------
    // Header + Tablet Dialogue
    // -------------------------
    private void RefreshHeader()
    {
        if (dayText) dayText.text = $"Day {day?.currentDay ?? 1}";
        if (moneyText) moneyText.text = $"Gold {economy?.money ?? 0}";
        if (repText) repText.text = $"Rep {(rep != null ? rep.reputation.ToString("0.00") : "2.50")}";
        if (innText) innText.text = $"Inn Lv {(innUpgrade != null ? innUpgrade.Level.ToString() : "1")}";
    }

    private void RefreshTabletDialogue()
    {
        if (flow == null) return;
        if (tabletDialogueText == null && tabletCustomerNameText == null) return;

        if (flow.TryGetCurrentOrderDialogue(out var profile, out var idx, out var cnt, out var line))
        {
            if (tabletCustomerNameText != null)
            {
                if (cnt <= 0)
                    tabletCustomerNameText.text = "";
                else
                    tabletCustomerNameText.text = string.IsNullOrWhiteSpace(profile.displayName) ? profile.id : profile.displayName;
            }

            if (tabletDialogueText != null)
                tabletDialogueText.text = line ?? "";
        }
    }

    // -------------------------
    // Buttons: Day / Customer / Order / Brewing
    // -------------------------
    public void OnClick_StartDay()
    {
        ResolveRefs();
        flow?.StartDay();

        // 정산 결과 뷰는 다음 날 시작하면 초기화
        _settlementDoneView = false;

        SetOpen(true);
        ShowPanel(p1_home);
    }

    public void OnClick_ReceiveCustomer()
    {
        ResolveRefs();
        flow?.ReceiveCustomer();

        SetOpen(true);
        ShowPanel(p1_home);
    }

    public void OnClick_ReceiveOrder()
    {
        ResolveRefs();
        flow?.OnClickReceiveOrder();

        // 주문받기 누르면 태블릿 닫기
        SetOpen(false);
        ShowPanel(p1_home);
    }

    public void OnClick_StartBrewing()
    {
        ResolveRefs();
        flow?.OnClickStartBrewing();

        // 조주하러가기 누르면 닫기
        SetOpen(false);
        ShowPanel(p1_home);
    }

    // -------------------------
    // Settlement Flow
    // -------------------------
    public void OnClick_Settlement()
    {
        ResolveRefs();

        // flow 내부에서 dayCycle.EndDayPublic() 호출 + dayPrepared=false로 내려감
        flow?.OnClickSettlement();

        // 이제 정산 결과를 보여주자
        _settlementDoneView = true;
        _lockToSettlementResult = true;

        SetOpen(true);
        ShowPanel(p2_settleResult);
        RefreshSettlementText();
    }

    public void OnClick_OpenStartDayPanelFromSettlement()
    {
        _lockToSettlementResult = false;
        _settlementDoneView = false;

        // 정산 결과 패널에서 "다음" 버튼으로 하루 시작 패널로 이동
        SetOpen(true);
        ShowPanel(p2_startDay);
    }

    private void RefreshSettlementText()
    {
        if (settlementText == null || ledger == null) return;

        settlementText.text =
            $"[정산]\n" +
            $"수익 변화: {ledger.IncomeDelta}\n" +
            $"평판 변화: {ledger.RepDelta:+0.00;-0.00}\n" +
            $"제공한 잔 수: {ledger.ServedDrinks}\n" +
            $"재운 인원: {ledger.SleptCustomers}\n" +
            $"손님 수: {ledger.ServedCustomers}";
    }

    // -------------------------
    // Rename (Flow가 OpenRename 호출)
    // -------------------------
    public void OpenRename(Drink drink, DrinkResult result, CustomerProfile customer, int slotIndex1Based, int slotCount)
    {
        SetOpen(true);
        ShowPanel(p1_rename);

        if (renameInfoText != null)
        {
            string cname = string.IsNullOrWhiteSpace(customer.displayName) ? customer.id : customer.displayName;
            renameInfoText.text =
                $"[이름 정하기]\n" +
                $"{cname} / 주문 {slotIndex1Based}/{slotCount}\n" +
                $"만족도: {result.satisfaction:0.#}\n" +
                $"도수: {drink.finalABV:0.#}%\n" +
                $"총량: {drink.totalMl:0.#}ml";
        }

        if (renameInput != null)
        {
            renameInput.text = "";
            renameInput.ActivateInputField();
        }
    }

    public void ConfirmRename()
    {
        ResolveRefs();

        string name = (renameInput != null) ? renameInput.text : "";
        flow?.ConfirmRenameAndContinue(name);

        // 이름 확정 후 태블릿 닫고, 다음 상태는 flow가 결정(손님받기/주문받기/정산하기)
        SetOpen(false);
        SyncPanelToState(true);
    }

    // -------------------------
    // Inn Decision (Sleep / Evict)
    // -------------------------
    public void OnClick_Sleep()
    {
        ResolveRefs();
        innDecision?.SleepOne();
        SyncPanelToState(true);
    }

    public void OnClick_Evict()
    {
        ResolveRefs();
        innDecision?.EvictOne();
        SyncPanelToState(true);
    }

    // -------------------------
    // Inn Upgrade Panels
    // -------------------------
    public void OnClick_OpenUpgradePanel()
    {
        ResolveRefs();
        if (innUpgrade == null) return;

        // 레벨에 따라 패널 선택
        if (innUpgrade.Level == 1)
        {
            ShowPanel(p2_upgrade12);
            FillUpgradeTexts(2);
            SetOpen(true);
        }
        else if (innUpgrade.Level == 2)
        {
            ShowPanel(p2_upgrade23);
            FillUpgradeTexts(3);
            SetOpen(true);
        }
        else
        {
            // 이미 Lv3이면 홈으로
            SetOpen(true);
            ShowPanel(p1_home);
        }
    }

    private void FillUpgradeTexts(int targetLevel)
    {
        if (innUpgrade == null) return;

        if (upgradeTitleText != null)
            upgradeTitleText.text = $"여관 업그레이드 Lv{targetLevel}";

        if (upgradeCostText != null)
            upgradeCostText.text = $"비용: {innUpgrade.NextCost}G";
    }

    public void OnClick_ConfirmUpgrade()
    {
        ResolveRefs();
        if (innUpgrade == null || economy == null) return;

        bool ok = innUpgrade.TryUpgrade(economy);
        RefreshHeader();

        // 성공/실패 상관없이 홈으로 복귀(원하면 실패 시 그대로 유지로 바꿔도 됨)
        SetOpen(true);
        ShowPanel(p1_home);

        if (!ok)
            Debug.Log("[Tablet] Upgrade failed (gold 부족 또는 max level).");
    }

    public void OnClick_CancelUpgrade()
    {
        SetOpen(true);
        ShowPanel(p1_home);
    }

    // -------------------------
    // Optional: Title / Exit
    // -------------------------
    public void OnClick_GoTitle()
    {
        SceneManager.LoadScene("TitleScene");
        SetOpen(false);
    }
}
