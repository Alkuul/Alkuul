using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Alkuul.Domain;
using Alkuul.Systems;
using Alkuul.UI;   // ResultUI

public class BrewingPanelBridge : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private BrewingSystem brewing;
    [SerializeField] private ServeSystem serve;
    [SerializeField] private DayCycleController dayCycle;
    [SerializeField] private ResultUI resultUI;

    [Header("Selections")]
    [SerializeField] private TechniqueSO technique;
    [SerializeField] private GlassSO glass;
    [SerializeField] private List<GarnishSO> garnishes = new();
    [SerializeField] private bool usesIce;

    [Header("Rules")]
    [Range(1, 3)][SerializeField] private int maxGarnishSlots = 1;
    [SerializeField] private bool requireTechnique = true;
    [SerializeField] private bool requireGlass = true;
    [SerializeField] private bool requireAtLeastOneGarnish = true;

    [Header("Reputation")]
    [Tooltip("만족도 표시가 0~135일 때도, 평판 임계값(81/61/...)을 기존 0~100 기준으로 유지")]
    [SerializeField] private bool scaleSatisfaction135To100ForRep = true;

    [SerializeField] private bool verboseLog = true;
    private void Log(string msg)
    {
        if (verboseLog) Debug.Log(msg);
    }
    // Session
    private CustomerProfile customer;
    private bool hasCustomer;

    private Order currentOrder;
    private bool hasOrder;

    public bool UsesIce => usesIce;
    public int CurrentPortionCount => brewing != null ? brewing.PortionCount : 0;
    public Alkuul.Domain.Drink PreviewDrink() => brewing != null ? brewing.Compute(usesIce) : default;


    // 결과 누적
    private readonly List<Drink> servedDrinks = new();
    private readonly List<DrinkResult> drinkResults = new();
    private bool leftEarly;

    // --------------------
    // Session API
    // --------------------

    private void Awake()
    {
        if (brewing == null) brewing = FindObjectOfType<BrewingSystem>();
        if (serve == null) serve = FindObjectOfType<ServeSystem>();
        if (dayCycle == null) dayCycle = FindObjectOfType<DayCycleController>();
        if (resultUI == null) resultUI = FindObjectOfType<ResultUI>();
    }
    private bool EnsureSystems()
    {
        // includeInactive=true (Unity 2020+)
        if (brewing == null) brewing = FindObjectOfType<Alkuul.Systems.BrewingSystem>(true);
        if (serve == null) serve = FindObjectOfType<Alkuul.Systems.ServeSystem>(true);

        if (brewing == null || serve == null)
        {
            Debug.LogError($"[BrewingPanelBridge] Missing refs. brewing={(brewing != null)} serve={(serve != null)} " +
                           $"(코어 DontDestroy 안에 시스템이 실제로 존재하는지 확인!)");
            return false;
        }
        return true;
    }

    public void BeginCustomer(CustomerProfile c)
    {
        customer = c;
        hasCustomer = true;
        hasOrder = false;

        servedDrinks.Clear();
        drinkResults.Clear();
        leftEarly = false;

        ResetMix();
    }

    public void BeginCustomer(CustomerProfile c, Order order)
    {
        BeginCustomer(c);
        SetOrder(order);
    }

    public void SetOrder(Order order)
    {
        currentOrder = order;
        hasOrder = true;
        ResetMix();
    }

    // 기존/다른 스크립트 대비 별칭
    public void SetCurrentOrder(Order order) => SetOrder(order);

    // --------------------
    // Inputs / UI bindings
    // --------------------
    public void SetIce(bool on)
    {
        usesIce = on;
        Log($"[Bridge] Ice={on}");
    }
    public void SetUsesIce(bool on) => SetIce(on);

    public void SelectTechnique(TechniqueSO t)
    {
        technique = t;
        Log($"[Bridge] Technique={(t ? t.name : "NULL")}");
    }
    public void SetTechnique(TechniqueSO t) => SelectTechnique(t);
    public void SelectGlass(GlassSO g)
    {
        glass = g;
        Log($"[Bridge] Glass={(g ? g.name : "NULL")}");
    }
    public void SetGlass(GlassSO g) => SelectGlass(g);

    // GarnishToggleBinder 호환 (GarnishSO, bool) -> bool
    public bool SetGarnishes(GarnishSO garnish, bool on)
    {
        if (garnish == null) { Log("[Bridge] Garnish=NULL"); return false; }

        if (!on)
        {
            garnishes.Remove(garnish);
            Log($"[Bridge] Garnish OFF: {garnish.name} (count={garnishes.Count}/{maxGarnishSlots})");
            return true;
        }

        if (garnishes.Contains(garnish))
        {
            Log($"[Bridge] Garnish already ON: {garnish.name} (count={garnishes.Count}/{maxGarnishSlots})");
            return true;
        }

        if (garnishes.Count >= maxGarnishSlots)
        {
            Log($"[Bridge] Garnish ON blocked(slot full): {garnish.name} (count={garnishes.Count}/{maxGarnishSlots})");
            return false;
        }

        garnishes.Add(garnish);
        Log($"[Bridge] Garnish ON: {garnish.name} (count={garnishes.Count}/{maxGarnishSlots})");
        return true;
    }

    public bool SetGarnish(GarnishSO garnish, bool on) => SetGarnishes(garnish, on);

    public void SetGarnishes(List<GarnishSO> list)
    {
        garnishes.Clear();
        if (list == null) return;

        foreach (var g in list)
        {
            if (g == null) continue;
            if (garnishes.Count >= maxGarnishSlots) break;
            if (!garnishes.Contains(g)) garnishes.Add(g);
        }
    }

    public void SetMaxGarnishSlots(int slots)
    {
        maxGarnishSlots = Mathf.Clamp(slots, 1, 3);
        if (garnishes.Count > maxGarnishSlots)
            garnishes.RemoveRange(maxGarnishSlots, garnishes.Count - maxGarnishSlots);
    }

    // Jigger 호환 엔트리포인트
    public void OnPortionAdded(IngredientSO ingredient, float ml)
    {
        if (!EnsureSystems()) return;
        if (ingredient == null || ml <= 0f) return;
        brewing.Add(ingredient, ml);
        Log($"[Bridge] AddPortion {ingredient.name} {ml}ml");
        Log($"[Bridge] AddPortion {ingredient.name} {ml}ml | count={brewing.PortionCount} | brewingID={brewing.GetInstanceID()} | bridgeID={GetInstanceID()}");
    }

    // 별칭
    public void AddPortion(IngredientSO ingredient, float ml) => OnPortionAdded(ingredient, ml);

    public void ResetMix()
    {
        brewing?.ResetMix();
        Log("[Bridge] ResetMix");
    }

    // --------------------
    // Serve / Finish
    // --------------------
    public void SubmitDrink() => ServeOnce(); // Unity Button용

    public DrinkResult ServeOnce()
    {
        if (!CanServe(out var reason))
        {
            Debug.LogWarning($"[BrewingPanelBridge] Serve blocked: {reason}");
            return default;
        }

        Log($"[Bridge] ServeOnce start (Ice={usesIce}, Tech={(technique ? technique.name : "NULL")}, Glass={(glass ? glass.name : "NULL")}, Garnish={garnishes.Count})");

        Drink d = brewing.Compute(usesIce);
        var meta = ServeSystem.Meta.From(technique, glass, garnishes, usesIce);

        var r = serve.ServeOne(currentOrder, d, meta, customer);

        servedDrinks.Add(d);
        drinkResults.Add(r);

        resultUI?.ShowDrinkResult(r);

        if (r.customerLeft) leftEarly = true;

        ResetMix();
        return r;
    }

    public void FinishCustomer()
    {
        if (!hasCustomer)
        {
            Debug.LogWarning("[BrewingPanelBridge] FinishCustomer: customer not set.");
            return;
        }
        if (drinkResults.Count == 0)
        {
            Debug.LogWarning("[BrewingPanelBridge] FinishCustomer: no drinks served.");
            return;
        }

        var cr = BuildCustomerResult();

        resultUI?.ShowCustomerResult(cr);
        dayCycle?.OnCustomerFinished(cr);

        hasCustomer = false;
        hasOrder = false;
    }

    private CustomerResult BuildCustomerResult()
    {
        float avg = drinkResults.Average(x => x.satisfaction);
        float avgRaw = drinkResults.Average(x => x.satisfactionRaw);
        int tipSum = drinkResults.Sum(x => x.tip);

        float repBasis = avg;
        if (scaleSatisfaction135To100ForRep)
            repBasis = (avg / 135f) * 100f;

        float repDelta = leftEarly ? -0.25f :
            (repBasis >= 81 ? 0.25f :
             repBasis >= 61 ? 0.1f :
             repBasis >= 41 ? 0f :
             repBasis >= 21 ? -0.25f : -0.5f);

        int intoxPoints = IntoxSystem.ComputePoints(servedDrinks, customer.tolerance);
        int intoxStage = IntoxSystem.GetStage(intoxPoints);

        bool isOver = intoxStage >= 5;
        bool canSleepAtInn = !leftEarly && intoxStage >= 4;

        return new CustomerResult
        {
            customerId = customer.id,
            drinkResults = new List<DrinkResult>(drinkResults),
            averageSatisfaction = avg,
            averageSatisfactionRaw = avgRaw,
            totalTip = tipSum,
            reputationDelta = repDelta,
            leftEarly = leftEarly,
            intoxPoints = intoxPoints,
            intoxStage = intoxStage,
            canSleepAtInn = canSleepAtInn,
            isOver = isOver
        };
    }

    private bool CanServe(out string reason)
    {
        if (brewing == null || serve == null) { reason = "brewing/serve refs missing"; return false; }
        if (!hasCustomer) { reason = "customer not set (BeginCustomer)"; return false; }
        if (!hasOrder) { reason = "order not set (SetOrder)"; return false; }
        if (requireTechnique && technique == null) { reason = "technique required"; return false; }
        if (requireGlass && glass == null) { reason = "glass required"; return false; }
        if (requireAtLeastOneGarnish && (garnishes == null || garnishes.Count < 1)) { reason = "garnish required"; return false; }
        reason = null;
        return true;
    }
}
