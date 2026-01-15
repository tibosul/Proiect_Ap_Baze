using System.Collections.Generic;
using System.Threading.Tasks;
using VolunteerSystem.Core.Entities;

namespace VolunteerSystem.Core.Interfaces
{
    public interface IPointsService
    {
        Task AwardPointsAsync(int volunteerId, int points, PointsReason reason, int? eventId = null);
        Task<List<PointsTransaction>> GetTransactionsAsync(int volunteerId);
        Task<int> GetTotalPointsAsync(int volunteerId);
    }
}
