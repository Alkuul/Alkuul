using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Alkuul.Systems;
using Alkuul.UI;

public class TabletController : MonoBehaviour
{
    [Header("Core Systems")]
    [SerializeField] private DayCycleController day;
    [SerializeField] private EconomySystem economy;
    [SerializeField] private RepSystem rep;
    [SerializeField] private InnUpgradeSystem innUpgrade;
    [SerializeField] private DailyLedgerSystem ledger;
    [SerializeField] private PendingInnDecisionSystem innDecision;

    [Header("Header UI")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text repText;
    [SerializeField] private TMP_Text innText;

    [Header("Pages")]
    [SerializeField] private GameObject pageHome;
    [SerializeField] private GameObject pageInnUpgrade;
    [SerializeField] private GameObject pageReputation;
    [SerializeField] private GameObject pageSettlement;
    [SerializeField] private GameObject pageSettings;
    [SerializeField] private GameObject pageRename;
    [SerializeField] private GameObject pageInnDecision;

    [Header("Root Toggle")]
    [SerializeField] private GameObject tabletRoot; // Tablet UI 전체 루트(열고/닫기)

    private void Awake()
    {
        // 코어에 있으니 Inspector 연결 권장. 비었으면 최소한 자동탐색(코어에서만)
        if (day == null) day = FindObjectOfType<DayCycleController>(true);
        if (economy == null) economy = FindObjectOfType<EconomySystem>(true);
        if (rep == null) rep = FindObjectOfType<RepSystem>(true);
        if (innUpgrade == null) innUpgrade = FindObjectOfType<InnUpgradeSystem>(true);
        if (ledger == null) ledger = FindObjectOfType<DailyLedgerSystem>(true);
        if (innDecision == null) innDecision = FindObjectOfType<PendingInnDecisionSystem>(true);

        ShowHome();
        SetOpen(false);
    }

    private void Update()
    {
        // 영상용: 헤더는 매 프레임 갱신해도 부담 거의 없음
        RefreshHeader();

        // 단축키(원하면)
        if (Input.GetKeyDown(KeyCode.Tab))
            SetOpen(!tabletRoot.activeSelf);
    }

    private void RefreshHeader()
    {
        if (dayText) dayText.text = $"Day {day?.currentDay ?? 1}";
        if (goldText) goldText.text = $"Gold {economy?.money ?? 0}";
        if (repText) repText.text = $"Rep {(rep != null ? rep.reputation.ToString("0.00") : "2.50")}";
        if (innText) innText.text = $"Inn Lv {(innUpgrade != null ? innUpgrade.Level.ToString() : "1")}";
    }

    public void SetOpen(bool open)
    {
        if (tabletRoot) tabletRoot.SetActive(open);
        if (open) ShowHome();
    }

    private void HideAllPages()
    {
        if (pageHome) pageHome.SetActive(false);
        if (pageInnUpgrade) pageInnUpgrade.SetActive(false);
        if (pageReputation) pageReputation.SetActive(false);
        if (pageSettlement) pageSettlement.SetActive(false);
        if (pageSettings) pageSettings.SetActive(false);
        if (pageRename) pageRename.SetActive(false);
        if (pageInnDecision) pageInnDecision.SetActive(false);
    }

    public void ShowHome()
    {
        HideAllPages();
        if (pageHome) pageHome.SetActive(true);
    }

    public void ShowInnUpgrade()
    {
        HideAllPages();
        if (pageInnUpgrade) pageInnUpgrade.SetActive(true);
    }

    public void ShowReputation()
    {
        HideAllPages();
        if (pageReputation) pageReputation.SetActive(true);
    }

    public void ShowSettlement()
    {
        HideAllPages();
        if (pageSettlement) pageSettlement.SetActive(true);
    }

    public void ShowSettings()
    {
        HideAllPages();
        if (pageSettings) pageSettings.SetActive(true);
    }

    public void ShowRename()
    {
        HideAllPages();
        if (pageRename) pageRename.SetActive(true);
    }

    public void ShowInnDecision()
    {
        HideAllPages();
        if (pageInnDecision) pageInnDecision.SetActive(true);
    }


    // 손님받기: OrderScene에 있는 InGameFlowController.StartDay() 호출
    public void OnClickReceiveCustomer()
    {
        var flow = FindObjectOfType<InGameFlowController>(true);
        if (flow == null)
        {
            Debug.LogWarning("[Tablet] InGameFlowController not found in current scene.");
            return;
        }
        flow.StartDay();
        SetOpen(false);
    }

    // 정산하기: 정산 페이지 열고, 다음날 버튼은 Settlement 페이지에서 처리
    public void OnClickSettlement()
    {
        ShowSettlement();
    }

    // 타이틀로: 씬 로드(코어는 남아있음) 필요하면 Reset도 붙이면 됨
    public void OnClickGoTitle()
    {
        SceneManager.LoadScene("TitleScene");
        SetOpen(false);
    }

    public void OnClickUpgradeInn()
    {
        if (innUpgrade == null || economy == null) return;
        innUpgrade.TryUpgrade(economy);
    }

    public void OnClickNextDay()
    {
        // 흐름은 InGameFlowController에 이미 OnClickNextDay()가 있음
        var flow = FindObjectOfType<InGameFlowController>(true);
        if (flow == null)
        {
            Debug.LogWarning("[Tablet] InGameFlowController not found.");
            return;
        }
        flow.OnClickNextDay();
        SetOpen(false);
    }

    public void OnClickSleep()
    {
        if (innDecision == null) return;
        innDecision.SleepOne(); // 아래 시스템에서 구현
    }

    public void OnClickEvict()
    {
        if (innDecision == null) return;
        innDecision.EvictOne();
    }
}
