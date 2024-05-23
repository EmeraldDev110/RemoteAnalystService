namespace RemoteAnalyst.BusinessLogic.Enums
{
    public static class Schedule
    {
        public enum Frequency
        {
            Daily = 1,
            Weekly = 2,
            Monthly = 3
        }

        public enum Types
        {
            Daily = 7,
            Weekly = 8,
            Monthly = 9,
            QuickTuner = 3,
            DeepDive = 4,
            Network = 6,
            Storage = 5,
            Application = 10,
            Pathway = 11,
            BatchSequence = 12
        }
    }
}