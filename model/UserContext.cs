namespace PizzaManagement.model
{
    public class UserContext
    {
        public float currentTime { get; set; }
        public bool IsWeekend { get; set; }
        public string? Weather { get; set; }
        public bool IsPromotionActive { get; set; }

        public UserContext()
        {
            currentTime = (float)DateTime.Now.Hour + (float)DateTime.Now.Minute / 60;
            IsWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday; 
        }
        public UserContext(DateTime historyTime)
        {
            currentTime = (float)historyTime.Hour + (float)historyTime.Minute / 60;
            IsWeekend = historyTime.DayOfWeek == DayOfWeek.Sunday || historyTime.DayOfWeek == DayOfWeek.Saturday;
        }

        public string GetTimeOfDay()
        {
            if (currentTime >= 1 && currentTime < 11) return "Morning";
            if (currentTime >= 11 && currentTime < 13) return "Noon";
            if (currentTime >= 13 && currentTime < 18) return "Afternoon";
            if (currentTime >= 18 && currentTime < 22) return "Evening";
            return "Night";
        }
    }
}