using System.Collections.Generic;
using UnityEngine;

namespace Alkuul.Domain
{
    [System.Serializable]
    public struct DrinkResult
    {
        public float satisfaction;
        public float satisfactionRaw;   // 내부 계산용, 100 초과 가능 (팁 계산 등)
        public int tip;
        public bool customerLeft;  // <40 즉시 이탈
    }

    [System.Serializable]
    public struct CustomerResult
    {
        public string customerId;
        public List<DrinkResult> drinkResults;
        public float averageSatisfaction;      // 0~100 평균
        public float averageSatisfactionRaw;   // 100 초과 포함한 평균
        public float reputationDelta;
        public int totalTip;
        public bool leftEarly;

        // 취함 관련
        public int intoxPoints;    // 누적 취함 포인트
        public int intoxStage;     // 1~5단계
        public bool canSleepAtInn; // 여관 재우기 가능(만취 이상 & 이탈 X & 오버 아님)
        public bool isOver;        // 오버(5단계 이상)
    }
}

