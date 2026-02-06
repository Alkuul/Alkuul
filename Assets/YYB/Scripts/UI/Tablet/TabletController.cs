using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Alkuul.Core;
using Alkuul.Domain;
using Alkuul.Systems;
using Alkuul.UI;

public class TabletController : MonoBehaviour
{
    [Header("Core Systems")]
    [SerializeField] private DayCycleController day;
    [SerializeField] private EconomySystem economy;
    [SerializeField] private RepSystem rep;
    [SerializeField] private InnSystem inn;

    [Header("Root Toggle")]
    [SerializeField] private GameObject tabletRoot;   // 실제로 숨길 루트(권장: Tablet 안에 별도 Root 오브젝트)
    [SerializeField] private bool autoOpenOnDayEnd = true;

    [Header("Pages")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;

    [Header("Page1 Panels")]
    [SerializeField] private GameObject p1_home;        // Page1/Panel1
    [SerializeField] private GameObject p1_rename;      // Page1/Panel2
    [SerializeField] private GameObject p1_innDecision; // Page1/Panel3

    [Header("Page2 Panels")]
    [SerializeField] private GameObject p2_settlePrompt;  // Page2/Panel1
    [SerializeField] private GameObject p2_settleResult;  // Page2/Panel2
    [SerializeField] private GameObject p2_dayStart;      // Page2/Panel3
    [SerializeField] private GameObject p2_upgrade12;     // Page2/Panel4
    [SerializeField] private GameObject p2_upgrade23;     // Page2/Panel5

    [Header("Texts (optional)")]
    [SerializeField] private TMP_Text headerDayText;
    [SerializeField] private TMP_Text headerGoldText;
    [SerializeField] private TMP_Text headerRepText;
    [SerializeField] private TMP_Text headerInnText;

    [SerializeField] private TMP_Text settlementText;

    [Header("Order Preview (optional)")]
    [SerializeField] private TMP_Text curCustomerNameText;
    [SerializeField] private TMP_Text curDialogueText;
    [SerializeField] private TMP_Text curOrderIndexText;

    [Header("Inn Upgrade (MVP)")]
    [SerializeField, Range(1, 3)] private int innLevel = 1;
    [SerializeField] private int costLv2 = 200;
    [SerializeField] private int costLv3 = 500;

    // ---- Daily ledger (MVP) ----
    private int dayStartMoney;
    private float dayStartRep;
    private int servedCustomers;
    private int servedDrinks;
    private int sleptCustomers;

    // 숙박 결정 대기열
    private readonly Queue<CustomerResult> pendingInn = new();

    private void Awake()
    {
        if (day == null) day = FindObjectOfType<DayCycleController>(true);
        if (economy == null) economy = FindObjectOfType<EconomySystem>(true);
        if (rep == null) rep = FindObjectOfType<RepSystem>(true);
        if (inn == null) inn = FindObjectOfType<InnSystem>(true);

        // 시작 화면(하루 시작 버튼)
        Show_P2_DayStart();
        SetOpen(false);
        RefreshHeader();
    }

    private void OnEnable()
    {
        EventBus.OnDayStarted += HandleDayStarted;
        EventBus.OnDayEnded += HandleDayEnded;
        EventBus.OnCustomerFinished += HandleCustomerFinished;
    }

    private void OnDisable()
    {
        EventBus.OnDayStarted -= HandleDayStarted;
        EventBus.OnDayEnded -= HandleDayEnded;
        EventBus.OnCustomerFinished -= HandleCustomerFinished;
    }

    private void Update()
    {
        RefreshHeader();

        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleOpen();

        if (Input.GetKeyDown(KeyCode.Escape) && tabletRoot != null && tabletRoot.activeSelf)
            Close();

        if (tabletRoot != null && tabletRoot.activeSelf)
            RefreshOrderPreview();
    }

    private void RefreshOrderPreview()
    {
        var flow = FindObjectOfType<InGameFlowController>(true);
        if (flow != null && flow.TryGetCurrentOrderDialogue(out var profile, out var idx, out var cnt, out var line))
        {
            if (curCustomerNameText) curCustomerNameText.text = string.IsNullOrWhiteSpace(profile.displayName) ? profile.id : profile.displayName;
            if (curOrderIndexText) curOrderIndexText.text = $"주문 {idx}/{cnt}";
            if (curDialogueText) curDialogueText.text = line ?? "";
        }
        else
        {
            if (curCustomerNameText) curCustomerNameText.text = "(대기 중)";
            if (curOrderIndexText) curOrderIndexText.text = "";
            if (curDialogueText) curDialogueText.text = "손님받기를 누르세요.";
        }
    }

    private bool IsOpen => tabletRoot != null && tabletRoot.activeSelf;

    // --------------------
    // Event handlers
    // --------------------
    private void HandleDayStarted()
    {
        dayStartMoney = economy ? economy.money : 0;
        dayStartRep = rep ? rep.reputation : 2.5f;

        servedCustomers = 0;
        servedDrinks = 0;
        sleptCustomers = 0;
        pendingInn.Clear();

        Show_P1_Home();
        SetOpen(false);
    }

    private void HandleCustomerFinished(CustomerResult cr)
    {
        servedCustomers++;
        servedDrinks += (cr.drinkResults != null ? cr.drinkResults.Count : 0);

        if (cr.canSleepAtInn)
        {
            pendingInn.Enqueue(cr);
            // “재우기/쫓아내기” 패널 띄우기
            Show_P1_InnDecision();
            SetOpen(true);
        }
    }

    private void HandleDayEnded()
    {
        // 마지막 잔 이후 = DayEnded
        Show_P2_SettlePrompt();
        if (autoOpenOnDayEnd) SetOpen(true);
    }

    // --------------------
    // UI open/close
    // --------------------
    public void SetOpen(bool open)
    {
        if (tabletRoot) tabletRoot.SetActive(open);
    }

    public void ToggleOpen()
    {
        Debug.Log("[Tablet] ToggleOpen clicked");
        if (!tabletRoot) return;
        SetOpen(!tabletRoot.activeSelf);
    }

    public void Close()
    {
        SetOpen(false);
    }

    public void Open()
    {
        SetOpen(true);
    }

    private void HideAllPanels()
    {
        if (page1) page1.SetActive(false);
        if (page2) page2.SetActive(false);

        if (p1_home) p1_home.SetActive(false);
        if (p1_rename) p1_rename.SetActive(false);
        if (p1_innDecision) p1_innDecision.SetActive(false);

        if (p2_settlePrompt) p2_settlePrompt.SetActive(false);
        if (p2_settleResult) p2_settleResult.SetActive(false);
        if (p2_dayStart) p2_dayStart.SetActive(false);
        if (p2_upgrade12) p2_upgrade12.SetActive(false);
        if (p2_upgrade23) p2_upgrade23.SetActive(false);
    }

    // --------------------
    // Show helpers (네 구조 그대로)
    // --------------------
    public void Show_P1_Home()
    {
        HideAllPanels();
        if (page1) page1.SetActive(true);
        if (p1_home) p1_home.SetActive(true);
    }

    public void Show_P1_Rename()
    {
        HideAllPanels();
        if (page1) page1.SetActive(true);
        if (p1_rename) p1_rename.SetActive(true);
    }

    public void Show_P1_InnDecision()
    {
        HideAllPanels();
        if (page1) page1.SetActive(true);
        if (p1_innDecision) p1_innDecision.SetActive(true);
    }

    public void Show_P2_SettlePrompt()
    {
        HideAllPanels();
        if (page2) page2.SetActive(true);
        if (p2_settlePrompt) p2_settlePrompt.SetActive(true);
    }

    public void Show_P2_SettleResult()
    {
        HideAllPanels();
        if (page2) page2.SetActive(true);
        if (p2_settleResult) p2_settleResult.SetActive(true);

        RefreshSettlementText();
    }

    public void Show_P2_DayStart()
    {
        HideAllPanels();
        if (page2) page2.SetActive(true);
        if (p2_dayStart) p2_dayStart.SetActive(true);
    }

    public void Show_P2_Upgrade()
    {
        HideAllPanels();
        if (page2) page2.SetActive(true);

        if (innLevel <= 1)
        {
            if (p2_upgrade12) p2_upgrade12.SetActive(true);
        }
        else if (innLevel == 2)
        {
            if (p2_upgrade23) p2_upgrade23.SetActive(true);
        }
        else
        {
            // 이미 Lv3면 홈으로 돌려도 되고, UI로 “최대 레벨” 표시해도 됨
            Show_P1_Home();
        }
    }

    // --------------------
    // Header / Settlement
    // --------------------
    private void RefreshHeader()
    {
        if (headerDayText) headerDayText.text = $"Day {day?.currentDay ?? 1}";
        if (headerGoldText) headerGoldText.text = $"Gold {economy?.money ?? 0}";
        if (headerRepText) headerRepText.text = $"Rep {(rep != null ? rep.reputation.ToString("0.00") : "2.50")}";
        if (headerInnText) headerInnText.text = $"Inn Lv {innLevel} (대기 {pendingInn.Count})";
    }

    private void RefreshSettlementText()
    {
        if (settlementText == null) return;

        int moneyNow = economy ? economy.money : 0;
        float repNow = rep ? rep.reputation : 2.5f;

        int incomeDelta = moneyNow - dayStartMoney;
        float repDelta = repNow - dayStartRep;

        settlementText.text =
            $"오늘 정산\n" +
            $"{repDelta: +0.00; -0.00}\n" +
            $"{incomeDelta}\n" +
            $"{sleptCustomers}\n" +
            $"{servedDrinks}\n";
    }

    // --------------------
    // Button callbacks (Inspector OnClick에 연결)
    // --------------------
    public void OnClick_ToggleTablet()
    {
        if (tabletRoot == null) return;
        tabletRoot.SetActive(!tabletRoot.activeSelf);
    }

    // “하루 시작” 버튼 (Page2/Panel3)
    public void OnClick_StartDay()
    {
        var flow = FindObjectOfType<InGameFlowController>(true);
        if (flow == null)
        {
            Debug.LogWarning("[Tablet] InGameFlowController not found.");
            return;
        }

        flow.StartDay();

        Show_P1_Home();
        SetOpen(true);
    }

    // “손님받기”를 홈에 두고 싶으면 이걸 연결
    public void OnClick_ReceiveCustomer()
    {
        var flow = FindObjectOfType<InGameFlowController>(true);
        if (flow == null)
        {
            Debug.LogWarning("[Tablet] InGameFlowController not found in current scene.");
            return;
        }

        flow.ReceiveCustomer();

        // 주문 화면 보이게 태블릿 닫고 싶으면:
        SetOpen(false);
    }

    // “정산하기” 버튼 (Page2/Panel1)
    public void OnClick_DoSettlement()
    {
        Show_P2_SettleResult();
    }

    // “다음 일차” 버튼(정산 결과 화면에서)
    public void OnClick_NextDay()
    {
        var flow = FindObjectOfType<InGameFlowController>(true);
        if (flow != null) flow.OnClickNextDay();
        else Debug.LogWarning("[Tablet] InGameFlowController not found.");

        Show_P2_DayStart();
        SetOpen(false);
    }

    // “재우기/쫓아내기” (Page1/Panel3)
    public void OnClick_Sleep()
    {
        if (inn == null) inn = FindObjectOfType<InnSystem>(true);
        if (inn == null) return;

        if (pendingInn.Count <= 0) { Show_P1_Home(); return; }

        var cr = pendingInn.Dequeue();
        bool ok = inn.TrySleep(cr);
        if (ok) sleptCustomers++;

        Show_P1_Home();
        RefreshSettlementText();
    }

    public void OnClick_Evict()
    {
        if (pendingInn.Count > 0) pendingInn.Dequeue();
        Show_P1_Home();
        RefreshSettlementText();
    }

    // 업그레이드 (Page2/Panel4, Panel5)
    public void OnClick_OpenUpgrade()
    {
        Show_P2_Upgrade();
        SetOpen(true);
    }

    public void OnClick_ConfirmUpgrade()
    {
        if (economy == null) return;

        int cost = innLevel == 1 ? costLv2 : (innLevel == 2 ? costLv3 : -1);
        if (cost < 0) { Show_P1_Home(); return; }

        if (economy.money < cost)
        {
            Debug.Log($"[Tablet] Not enough gold. need={cost} have={economy.money}");
            return;
        }

        economy.money -= cost;
        innLevel = Mathf.Clamp(innLevel + 1, 1, 3);

        // 조주쪽 브릿지에 가니쉬 슬롯 반영
        var bridge = FindObjectOfType<BrewingPanelBridge>(true);
        if (bridge != null) bridge.SetMaxGarnishSlots(innLevel);

        Show_P1_Home();
    }

    // 타이틀로
    public void OnClick_GoTitle()
    {
        SceneManager.LoadScene("TitleScene");
        SetOpen(false);
    }
}
