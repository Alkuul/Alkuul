namespace Alkuul.UI
{
    public static class PrototypeEndingContext
    {
        public static int FinalDay { get; set; }
        public static int TotalCustomers { get; set; }
        public static float AverageSatisfaction { get; set; }

        public static void Clear()
        {
            FinalDay = 0;
            TotalCustomers = 0;
            AverageSatisfaction = 0f;
        }
    }
}