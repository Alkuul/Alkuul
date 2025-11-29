using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Alkuul.Domain;

namespace Alkuul.Systems
{
    /// <summary>한 잔 / 한 손님 평가 집계</summary>
    public sealed class ServeSystem : MonoBehaviour
    {
        [SerializeField] private ScoringService scoring;

        public struct Meta
        {
            public string[] techniqueTags;
            public string[] glassTags;
            public string[] garnishTags;
            public bool usesIce;

            public static Meta From(
                TechniqueSO technique,
                GlassSO glass,
                IEnumerable<GarnishSO> garnishes,
                bool usesIce
            )
            {
                return new Meta
                {
                    techniqueTags = technique?.tags,
                    glassTags = glass?.tags,
                    garnishTags = garnishes != null
                        ? System.Linq.Enumerable.SelectMany(garnishes, g => g.tags).ToArray()
                        : null,
                    usesIce = usesIce
                };
            }
        }

        public DrinkResult ServeOne(Order order, Drink drink, Meta meta, CustomerProfile customer)
        {
            return scoring.Evaluate(order, drink, meta, customer);
        }

        /// <summary>
        /// 한 손님(최대 3잔) 전체 평가 + 취함/여관 여부 계산
        /// </summary>
        public CustomerResult ServeCustomer(
            CustomerProfile customer,
            Order order,
            IEnumerable<(Drink drink, Meta meta)> drinks
        )
        {
            var results = new List<DrinkResult>();
            var drinkList = new List<Drink>();
            bool early = false;

            foreach (var (drink, meta) in drinks)
            {
                var r = scoring.Evaluate(order, drink, meta, customer);
                results.Add(r);
                drinkList.Add(drink);

                if (r.customerLeft)
                {
                    early = true;
                    break;
                }

                if (results.Count >= 3)
                    break; // 3잔 초과 X
            }

            float avg = results.Count > 0 ? results.Average(x => x.satisfaction) : 0f;
            float avgRaw = results.Count > 0 ? results.Average(x => x.satisfactionRaw) : 0f;
            int tipSum = results.Sum(x => x.tip);

            float repDelta = early ? -0.25f :
                (avg >= 81 ? 0.25f :
                 avg >= 61 ? 0.1f :
                 avg >= 41 ? 0f :
                 avg >= 21 ? -0.25f : -0.5f);

            // 취함 계산
            int intoxPoints = IntoxSystem.ComputePoints(drinkList, customer.tolerance);
            int intoxStage = IntoxSystem.GetStage(intoxPoints);

            bool isOver = intoxStage >= 5;
            bool canSleepAtInn = !early && intoxStage >= 4; // 만취(4) 이상이면 재우기 가능

            return new CustomerResult
            {
                customerId = customer.id,
                drinkResults = results,
                averageSatisfaction = avg,
                averageSatisfactionRaw = avgRaw,
                totalTip = tipSum,
                reputationDelta = repDelta,
                leftEarly = early,
                intoxPoints = intoxPoints,
                intoxStage = intoxStage,
                canSleepAtInn = canSleepAtInn,
                isOver = isOver
            };
        }
    }
}
