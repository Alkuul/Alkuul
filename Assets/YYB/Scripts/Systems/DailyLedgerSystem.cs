using UnityEngine;
using Alkuul.Core;
using Alkuul.Domain;
using Alkuul.Systems;

public class DailyLedgerSystem : MonoBehaviour
{
    [SerializeField] private EconomySystem economy;
    [SerializeField] private RepSystem rep;

    private int dayStartMoney;
    private float dayStartRep;

    public int ServedCustomers { get; private set; }
    public int ServedDrinks { get; private set; }
    public int SleptCustomers { get; private set; }  // 재우기 성공 수

    public int IncomeDelta => (economy ? economy.money : 0) - dayStartMoney;
    public float RepDelta => (rep ? rep.reputation : 2.5f) - dayStartRep;

    private void OnEnable()
    {
        if (economy == null) economy = FindObjectOfType<EconomySystem>(true);
        if (rep == null) rep = FindObjectOfType<RepSystem>(true);

        EventBus.OnDayStarted += HandleDayStarted;
        EventBus.OnDayEnded += HandleDayEnded;
    }

    private void OnDisable()
    {
        EventBus.OnDayStarted -= HandleDayStarted;
        EventBus.OnDayEnded -= HandleDayEnded;
    }

    private void HandleDayStarted()
    {
        dayStartMoney = economy ? economy.money : 0;
        dayStartRep = rep ? rep.reputation : 2.5f;

        ServedCustomers = 0;
        ServedDrinks = 0;
        SleptCustomers = 0;

        Debug.Log("[Ledger] Day Started snapshot");
    }

    private void HandleDayEnded()
    {
        Debug.Log($"[Ledger] Day Ended incomeΔ={IncomeDelta} repΔ={RepDelta:+0.00;-0.00} drinks={ServedDrinks} slept={SleptCustomers}");
    }

    public void RecordCustomer(CustomerResult cr)
    {
        ServedCustomers++;
        ServedDrinks += (cr.drinkResults != null ? cr.drinkResults.Count : 0);
    }

    public void RecordSleepSuccess()
    {
        SleptCustomers++;
    }
}
