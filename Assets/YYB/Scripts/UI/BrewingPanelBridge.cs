using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Alkuul.Domain;
using Alkuul.Systems;
using Alkuul.UI; // ResultUI

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
    [Tooltip("평판 계산 시 satisfaction(0~135)을 0~100 기준으로 스케일")]
    [SerializeField] private bool scaleSatisfaction135To100ForRep = true;

    [SerializeField] private bool verboseLog = true;

    public event Action<GlassSO> GlassChanged;
    public event Action<IReadOnlyList<GarnishSO>> GarnishesChanged;


    // Session
    private CustomerProfile customer;
    private bool hasCustomer;

    private Order currentOrder;
    private bool hasOrder;

    public bool UsesIce => usesIce;
    public int CurrentPortionCount => brewing != null ? brewing.PortionCount : 0;
    public Drink PreviewDrink() => brewing != null ? brewing.Compute(usesIce) : default;
    public GlassSO SelectedGlass => glass;
    public IReadOnlyList<GarnishSO> SelectedGarnishes => garnishes;

    // served history
    private readonly List<Drink> servedDrinks = new();
    private readonly List<DrinkResult> drinkResults = new();
    private bool leftEarly;

    private Drink _lastServedDrink;
    private DrinkResult _lastDrinkResult;
    private bool _hasLastServed;

    private void Awake()
    {
        if (brewing == null) brewing = FindObjectOfType<BrewingSystem>(true);
        if (serve == null) serve = FindObjectOfType<ServeSystem>(true);
        if (dayCycle == null) dayCycle = FindObjectOfType<DayCycleController>(true);
        if (resultUI == null) resultUI = FindObjectOfType<ResultUI>(true);

        // 가니쉬 슬롯은 여관 업그레이드 따라가게(있으면)
        var innUp = FindObjectOfType<InnUpgradeSystem>(true);
        if (innUp != null) SetMaxGarnishSlots(innUp.MaxGarnishSlots);
    }

    private void Log(string msg)
    {
        if (verboseLog) Debug.Log(msg);
    }

    public bool TryGetLastServed(out Drink drink, out DrinkResult result)
    {
        if (_hasLastServed)
        {
            drink = _lastServedDrink;
            result = _lastDrinkResult;
            return true;
        }

        drink = default;
        result = default;
        return false;
    }

    public void BeginCustomer(CustomerProfile c)
    {
        customer = c;
        hasCustomer = true;
        hasOrder = false;

        servedDrinks.Clear();
        drinkResults.Clear();
        leftEarly = false;

        _hasLastServed = false;

        ResetMix();
    }

    public void RestoreSession(IReadOnlyList<Drink> drinks, IReadOnlyList<DrinkResult> results, bool left)
    {
        servedDrinks.Clear();
        drinkResults.Clear();

        if (drinks != null) servedDrinks.AddRange(drinks);
        if (results != null) drinkResults.AddRange(results);

        leftEarly = left;
        _hasLastServed = false;
    }

    public void SetOrder(Order order)
    {
        currentOrder = order;
        hasOrder = true;
        ResetMix();
    }

    public void SetCurrentOrder(Order order) => SetOrder(order);

    // ---- UI bindings ----
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
        GlassChanged?.Invoke(glass);
        Log($"[Bridge] Glass={(g ? g.name : "NULL")}");
    }
    public void SetGlass(GlassSO g) => SelectGlass(g);

    // Garnish toggle binder (GarnishSO, bool) -> bool
    public bool SetGarnishes(GarnishSO garnish, bool on)
    {
        if (garnish == null) { Log("[Bridge] Garnish=NULL"); return false; }

        if (!on)
        {
            garnishes.Remove(garnish);
            Log($"[Bridge] Garnish OFF: {garnish.name} (count={garnishes.Count}/{maxGarnishSlots})");
            GarnishesChanged?.Invoke(garnishes);
            return true;
        }

        if (garnishes.Contains(garnish)) return true;

        if (garnishes.Count >= maxGarnishSlots)
        {
            Log($"[Bridge] Garnish blocked(slot full): {garnish.name}");
            return false;
        }

        garnishes.Add(garnish);
        Log($"[Bridge] Garnish ON: {garnish.name} (count={garnishes.Count}/{maxGarnishSlots})");
        GarnishesChanged?.Invoke(garnishes);
        return true;
    }

    public bool SetGarnish(GarnishSO garnish, bool on) => SetGarnishes(garnish, on);

    public void SetMaxGarnishSlots(int slots)
    {
        maxGarnishSlots = Mathf.Clamp(slots, 1, 3);
        if (garnishes.Count > maxGarnishSlots)
            garnishes.RemoveRange(maxGarnishSlots, garnishes.Count - maxGarnishSlots);
        GarnishesChanged?.Invoke(garnishes);
    }

    public void OnPortionAdded(IngredientSO ingredient, float ml)
    {
        if (brewing == null) brewing = FindObjectOfType<BrewingSystem>(true);
        if (brewing == null) return;

        if (ingredient == null || ml <= 0f) return;
        brewing.Add(ingredient, ml);
        Log($"[Bridge] AddPortion {ingredient.name} {ml}ml | count={brewing.PortionCount}");
    }

    public void AddPortion(IngredientSO ingredient, float ml) => OnPortionAdded(ingredient, ml);

    public void ResetMix()
    {
        brewing?.ResetMix();
        garnishes.Clear();
        Log("[Bridge] ResetMix");
    }

    // ---- Serve / Finish ----
    public DrinkResult ServeOnce()
    {
        if (!CanServe(out var reason))
        {
            Debug.LogWarning($"[Bridge] Serve blocked: {reason}");
            return default;
        }

        Drink d = brewing.Compute(usesIce);
        var meta = ServeSystem.Meta.From(technique, glass, garnishes, usesIce);

        var r = serve.ServeOne(currentOrder, d, meta, customer);

        servedDrinks.Add(d);
        drinkResults.Add(r);

        _lastServedDrink = d;
        _lastDrinkResult = r;
        _hasLastServed = true;

        resultUI?.ShowDrinkResult(r);

        if (r.customerLeft) leftEarly = true;

        ResetMix();
        return r;
    }

    public void FinishCustomer()
    {
        if (!hasCustomer)
        {
            Debug.LogWarning("[Bridge] FinishCustomer: customer not set.");
            return;
        }
        if (drinkResults.Count == 0)
        {
            Debug.LogWarning("[Bridge] FinishCustomer: no drinks served.");
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
