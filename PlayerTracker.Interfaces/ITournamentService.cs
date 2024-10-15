using PlayerTracker.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlayerTracker.Interfaces
{
    public interface ITournamentService
    {
        Task<IEnumerable<Tournament>?> GetMostRecentTournaments(int year);
    }
}
