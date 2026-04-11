using UnityEngine;

namespace Alkuul.UI
{
    public static class DayStartContinueSave
    {
        public struct SaveData
        {
            public int day;
            public int money;
            public float reputation;
            public int innLevel;

            public int totalCustomersOverall;
            public int totalDrinksOverall;
            public float totalAverageSatisfactionSum;
        }

        private const string KeyHasSave = "continue.hasSave";
        private const string KeyDay = "continue.day";
        private const string KeyMoney = "continue.money";
        private const string KeyReputation = "continue.rep";
        private const string KeyInnLevel = "continue.innLevel";
        private const string KeyTotalCustomers = "continue.totalCustomers";
        private const string KeyTotalDrinks = "continue.totalDrinks";
        private const string KeyTotalAverageSatSum = "continue.totalAvgSatSum";

        private static bool _continueRequested;

        public static void RequestContinueLoad()
        {
            _continueRequested = true;
        }

        public static bool ConsumeContinueRequest()
        {
            if (!_continueRequested) return false;
            _continueRequested = false;
            return true;
        }

        public static bool HasSave()
        {
            return PlayerPrefs.GetInt(KeyHasSave, 0) == 1;
        }

        public static void Save(SaveData data)
        {
            PlayerPrefs.SetInt(KeyHasSave, 1);
            PlayerPrefs.SetInt(KeyDay, data.day);
            PlayerPrefs.SetInt(KeyMoney, data.money);
            PlayerPrefs.SetFloat(KeyReputation, data.reputation);
            PlayerPrefs.SetInt(KeyInnLevel, data.innLevel);

            PlayerPrefs.SetInt(KeyTotalCustomers, data.totalCustomersOverall);
            PlayerPrefs.SetInt(KeyTotalDrinks, data.totalDrinksOverall);
            PlayerPrefs.SetFloat(KeyTotalAverageSatSum, data.totalAverageSatisfactionSum);

            PlayerPrefs.Save();

            Debug.Log($"[ContinueSave] Saved day={data.day}, money={data.money}, rep={data.reputation:0.00}, innLv={data.innLevel}");
        }

        public static bool TryLoad(out SaveData data)
        {
            data = default;

            if (!HasSave())
                return false;

            data.day = PlayerPrefs.GetInt(KeyDay, 1);
            data.money = PlayerPrefs.GetInt(KeyMoney, 0);
            data.reputation = PlayerPrefs.GetFloat(KeyReputation, 2.5f);
            data.innLevel = PlayerPrefs.GetInt(KeyInnLevel, 1);

            data.totalCustomersOverall = PlayerPrefs.GetInt(KeyTotalCustomers, 0);
            data.totalDrinksOverall = PlayerPrefs.GetInt(KeyTotalDrinks, 0);
            data.totalAverageSatisfactionSum = PlayerPrefs.GetFloat(KeyTotalAverageSatSum, 0f);

            return true;
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(KeyHasSave);
            PlayerPrefs.DeleteKey(KeyDay);
            PlayerPrefs.DeleteKey(KeyMoney);
            PlayerPrefs.DeleteKey(KeyReputation);
            PlayerPrefs.DeleteKey(KeyInnLevel);
            PlayerPrefs.DeleteKey(KeyTotalCustomers);
            PlayerPrefs.DeleteKey(KeyTotalDrinks);
            PlayerPrefs.DeleteKey(KeyTotalAverageSatSum);
            PlayerPrefs.Save();

            Debug.Log("[ContinueSave] Cleared.");
        }
    }
}