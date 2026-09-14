using PizzaManagement.AI.Models;

namespace PizzaManagement.AI.Predictors
{
    public class ActionPredictor
    {
        private readonly QTable _qTable;
        private readonly Random _random = new Random();

        private double epsilon = 0.1;
        public ActionPredictor(QTable qTable)
        {
            _qTable = qTable;
        }

        public List<string> GetRankActions(string stateKey, List<string> possibleAction)
        {
            if (possibleAction == null || !possibleAction.Any())
            {
                return new List<string>();
            }

            if (_random.NextDouble() < epsilon)
            {
                return possibleAction.OrderByDescending(x => _random.Next()).ToList();
            }

            if (!_qTable.Table.ContainsKey(stateKey))
                return possibleAction.OrderBy(x => _random.Next()).ToList();


            var actionScores = _qTable.Table[stateKey];
            return possibleAction
                .OrderByDescending(action =>
                {
                    var score = actionScores.ContainsKey(action)
                    ? actionScores[action]
                    : 0;
                    
                return score + (_random.NextDouble() * 0.3);
                }).ToList();
        }

        public double GetScore(string stateKey, string action)
        {
            if(!_qTable.Table.ContainsKey(stateKey))
            {
                return 0;
            }
            if (!_qTable.Table[stateKey].ContainsKey(action))
            {
                return 0;
            }
            return _qTable.Table[stateKey][action];
        }
    }
}
