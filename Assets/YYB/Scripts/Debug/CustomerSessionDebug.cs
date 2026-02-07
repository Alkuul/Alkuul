using System.Collections.Generic;
using UnityEngine;
using Alkuul.Domain;
using Alkuul.Systems;
using Alkuul.UI;

namespace Alkuul.Dev
{
    public class CustomerSessionDebug : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private OrderSystem orderSystem;
        [SerializeField] private BrewingSystem brewingSystem;
        [SerializeField] private ServeSystem serveSystem;
        [SerializeField] private DayCycleController dayCycle;
        [SerializeField] private ResultUI resultUI;
        [SerializeField] private BrewingUI brewingUI;

        [Header("Order")]
        [SerializeField] private List<SecondaryEmotionSO> keywords = new();
        [SerializeField] private Vector2 abvRange = new Vector2(0, 100);
        [SerializeField] private float timeLimit = 60f;

        [Header("Customer Profile")]
        [SerializeField] private string customerId = "debug_customer";
        [SerializeField] private string customerName = "Test Customer";
        [SerializeField] private Sprite customerPortrait;
        [SerializeField] private Tolerance tolerance = Tolerance.Normal;
        [SerializeField] private IcePreference icePreference = IcePreference.Neutral;

        private readonly List<(Drink drink, ServeSystem.Meta meta)> _servedDrinks = new();
        private Order _currentOrder;
        private CustomerProfile _currentCustomer;
        private bool _finished;

        private void Start()
        {
            CreateOrderAndCustomer();
        }

        private void CreateOrderAndCustomer()
        {
            if (orderSystem == null)
            {
                Debug.LogWarning("CustomerSessionDebug: OrderSystem missing.");
                return;
            }

            _currentOrder = orderSystem.CreateOrder(keywords, abvRange, timeLimit);
            _currentCustomer = new CustomerProfile
            {
                id = customerId,
                displayName = customerName,
                portrait = customerPortrait,
                tolerance = tolerance,
                icePreference = icePreference
            };

            _servedDrinks.Clear();
            _finished = false;

            if (brewingUI != null)
                brewingUI.ResetUI();

            Debug.Log($"[CustomerSession] New customer: {_currentCustomer.displayName}");
        }

        public void ServeOneDrinkFromBrewingUI()
        {
            if (_finished)
            {
                Debug.LogWarning("[CustomerSession] Customer already finished.");
                return;
            }

            if (brewingSystem == null || serveSystem == null || brewingUI == null)
            {
                Debug.LogWarning("CustomerSessionDebug: refs missing.");
                return;
            }

            Drink d = brewingSystem.Compute(brewingUI.UseIce);
            var meta = ServeSystem.Meta.From(
                brewingUI.SelectedTechnique,
                brewingUI.SelectedGlass,
                brewingUI.SelectedGarnishes,
                brewingUI.UseIce
            );

            var drinkResult = serveSystem.ServeOne(_currentOrder, d, meta, _currentCustomer);
            _servedDrinks.Add((d, meta));

            resultUI?.ShowDrinkResult(drinkResult);

            Debug.Log($"[CustomerSession] Served drink {_servedDrinks.Count}");

            if (drinkResult.customerLeft || _servedDrinks.Count >= 3)
            {
                Debug.Log("[CustomerSession] Auto finishing customer.");
                FinishCustomer();
                return;
            }

            brewingUI.ResetUI();
        }

        public void FinishCustomer()
        {
            if (_finished)
            {
                Debug.LogWarning("[CustomerSession] Already finished.");
                return;
            }

            if (serveSystem == null)
            {
                Debug.LogWarning("CustomerSessionDebug: ServeSystem missing.");
                return;
            }
            if (_servedDrinks.Count == 0)
            {
                Debug.LogWarning("[CustomerSession] No drinks served.");
                return;
            }

            var cr = serveSystem.ServeCustomer(_currentCustomer, _currentOrder, _servedDrinks);
            resultUI?.ShowCustomerResult(cr);

            dayCycle?.OnCustomerFinished(cr);

            _finished = true;

            Debug.Log("[CustomerSession] Customer finished.");
        }

        public void NextCustomer()
        {
            CreateOrderAndCustomer();
        }
    }
}
