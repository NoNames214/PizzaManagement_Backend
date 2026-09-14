using Microsoft.EntityFrameworkCore;
using PizzaManagement.AI.Models;
using PizzaManagement.Data;
using PizzaManagement.DataResponse;
using PizzaManagement.model;

namespace PizzaManagement.Service
{
    public class RecommendationService
    {
        private readonly AppDbContext _db;
        private readonly AIService _ai;

        public RecommendationService(AppDbContext db, AIService ai)
        {
            _db = db;
            _ai = ai;
        }

        public async Task<List<RecommendPizzaResponse>> GetRecommendationsAsync(int userId)
        {
            var context = new UserContext();
            string timeSlot = context.GetTimeOfDay();

            var state = new UserState
            {
                UserID = userId,
                TimeOrder = DateTime.Now,
            };
            var actions = await _db.Pizza
                .Where(p => !p.isDeleted)
                .AsNoTracking()
                .Select(p => p.Id.ToString())
                .ToListAsync();

            var rankedIds = _ai.GetRecommend(state, timeSlot, actions);

            var top3Ids = rankedIds.Take(3).ToList();
            var remainingIds = rankedIds.Skip(3).ToList();

            string randomId = "";
            if (remainingIds.Any())
            {
                var random = new Random();
                randomId = remainingIds[random.Next(remainingIds.Count)];
            }

            var finalIds = new List<string>(top3Ids);
            if (!string.IsNullOrEmpty(randomId))
            {
                finalIds.Add(randomId);
            }

            var finalPizzaIds = finalIds
                .Select(id => int.TryParse(id, out int parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .ToList();


            var pizzaNames = await _db.Pizza
                .Where(p => finalPizzaIds.Contains(p.Id))
                .AsNoTracking()
                .ToListAsync();

            var sortedPizzas = pizzaNames
                .OrderBy(p => finalPizzaIds.IndexOf(p.Id))
                .Select(p => new RecommendPizzaResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Image = p.Image,
                    Score = _ai.GetScore(state, timeSlot, p.Id.ToString())
                })
                .ToList();

            return sortedPizzas;
        }

        public async Task TrackActionAsync(int userId, int pizzaId, string interactiontype)
        {
            string normalizedType = (interactiontype ?? "").ToLower();
            var context = new UserContext();
            string timeSlot = context.GetTimeOfDay();

            double reward = normalizedType switch
            {
                "purchase" => 10.0,
                "add_to_cart" => 5.0,
                "click" => 1.0,
                "dismiss" => -5.0,
                _ => 0.0
            };

            var state = new UserState
            {
                UserID = userId,
                TimeOrder = DateTime.Now
            };

            var interaction = new UserInteraction
            {
                UserId = userId,
                PizzaId = pizzaId,
                InteractionType = normalizedType,
                Reward = (float)reward,
                Timestamp = DateTime.Now,
                IsTrained = false
            };
            _db.UserInteractions.Add(interaction);
            await _db.SaveChangesAsync();
            _ai.Train(state, timeSlot, pizzaId.ToString(), reward);
        }
    }
}
