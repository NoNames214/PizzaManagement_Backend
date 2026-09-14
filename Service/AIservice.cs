using PizzaManagement.AI.Models;
using PizzaManagement.AI.Predictors;
using PizzaManagement.AI.Trainners;

namespace PizzaManagement.Service
{
    public class AIService
    {
        private readonly ActionPredictor _actionPredictor;
        private readonly MLTrainer _mLTrainer;

        public AIService(ActionPredictor actionPredictor, MLTrainer mLTrainer)
        {
            _actionPredictor = actionPredictor;
            _mLTrainer = mLTrainer;
        }

        public List<string> GetRecommend(UserState state, string timeSlot, List<string> actions)
        {
            return _actionPredictor.GetRankActions(state.Tokey(timeSlot), actions);
        }

        public void Train(UserState state, string timeSlot, string action, double reward)
        {
            _mLTrainer.Update(state.Tokey(timeSlot), action, reward);
        }
        public double GetScore(UserState state, string timeSlot, string action)
        {
            return _actionPredictor.GetScore(state.Tokey(timeSlot), action);
        }
    }
}
