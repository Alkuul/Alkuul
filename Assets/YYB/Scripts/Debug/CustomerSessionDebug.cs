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
        [SerializeField] private string customerName = "테스트 손님";
        [SerializeField] private Tolerance tolerance = Tolerance.Normal;
        [SerializeField] private IcePreference icePreference = IcePreference.Neutral;
        [SerializeField] private CustomerPortraitSet portraitSet;

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
                Debug.LogWarning("CustomerSessionDebug: OrderSystem이 비어있습니다.");
                return;
            }

            _currentOrder = orderSystem.CreateOrder(keywords, abvRange, timeLimit);
            _currentCustomer = new CustomerProfile
            {
                id = customerId,
                displayName = customerName,
                tolerance = tolerance,
                icePreference = icePreference,
                portraitSet = portraitSet
            };

            _servedDrinks.Clear();
            _finished = false;

            if (brewingUI != null)
                brewingUI.ResetUI();

            Debug.Log($"[CustomerSession] 새 손님 입장: {_currentCustomer.displayName}");
        }

        public void ServeOneDrinkFromBrewingUI()
        {
            if (_finished)
            {
                Debug.LogWarning("[CustomerSession] 이미 손님 정산이 끝났습니다. FinishCustomer를 눌러 다음 손님으로 넘어가세요.");
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

            Debug.Log($"[CustomerSession] 잔 {_servedDrinks.Count} 서빙 완료");

            // 3잔 채우거나, 이번 잔에서 떠났으면 자동으로 정산
            if (drinkResult.customerLeft || _servedDrinks.Count >= 3)
            {
                Debug.Log("[CustomerSession] 자동으로 손님 정산을 진행합니다.");
                FinishCustomer();
                return;
            }

            // 다음 잔을 위해 리셋
            brewingUI.ResetUI();
        }

        public void FinishCustomer()
        {
            if (_finished)
            {
                Debug.LogWarning("[CustomerSession] 이미 정산 완료됨.");
                return;
            }

            if (serveSystem == null)
            {
                Debug.LogWarning("CustomerSessionDebug: ServeSystem 없음.");
                return;
            }
            if (_servedDrinks.Count == 0)
            {
                Debug.LogWarning("[CustomerSession] 제공한 잔이 없습니다.");
                return;
            }

            var cr = serveSystem.ServeCustomer(_currentCustomer, _currentOrder, _servedDrinks);
            resultUI?.ShowCustomerResult(cr);

            dayCycle?.OnCustomerFinished(cr);

            _finished = true;

            Debug.Log("[CustomerSession] 손님 정산 완료. 버튼으로 다음 손님/다음 Day를 진행할 수 있습니다.");

            // 다음 손님 준비는 DayCycle/버튼에서 호출하는 걸로 둠
        }

        public void NextCustomer()
        {
            CreateOrderAndCustomer();
        }
    }
}
