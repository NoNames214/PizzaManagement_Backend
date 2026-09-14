namespace PizzaManagement.AI.Models
{

    public class UserState
    {
        public int UserID { get; set; }

        public DateTime TimeOrder { get; set; }

        public string Tokey(string timeSlot)
        {

            return $"{UserID}_{timeSlot}";
        }
    }
}
