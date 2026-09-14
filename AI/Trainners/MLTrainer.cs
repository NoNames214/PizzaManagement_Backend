using PizzaManagement.AI.Models;
using PizzaManagement.model;

namespace PizzaManagement.AI.Trainners
{

    public class MLTrainer
    {
        public QTable _qTable;


        private double alpha = 0.5; 

        public MLTrainer(QTable qTable) {
            _qTable = qTable;
        }

        public void Update(string state, string action, double reward)
        {
           
            if (!_qTable.Table.ContainsKey(state))
            {
                _qTable.Table[state] = new Dictionary<string, double>();
            }

           
            if (!_qTable.Table[state].ContainsKey(action))
            {
                 _qTable.Table[state][action] = 0;
            }

            
            var currentQ = _qTable.Table[state][action];

           
            var newcurrentQ = currentQ + alpha * (reward - currentQ);

            _qTable.Table[state][action] = newcurrentQ;

        }
        public void BuildFromDb(List<UserInteraction> interactions)
        {
            _qTable.Table.Clear();
            var sortedInteractions = interactions
                .OrderBy(interactions => interactions.Timestamp).ToList();

            foreach (var item in sortedInteractions)
            {
                var context = new UserContext(item.Timestamp);
                string historyTimeSlot = context.GetTimeOfDay();

                var userState = new UserState
                {
                    UserID = item.UserId,
                };

                string stateKey = userState.Tokey(historyTimeSlot);

                Update(
                    stateKey,
                    item.PizzaId.ToString(),
                    item.Reward
                );
            }           
        }
    }

}
