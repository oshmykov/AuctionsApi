using AuctionsApi.Business.Models.Objects;
using System.Threading.Tasks;

namespace AuctionsApi.Business.Models.Abstract
{
    public interface IParticipantsService
    {
        Task<ParticipantInfo> GetParticipantInfo(string participantId);

        Task<IPlainCommandResult> JoinAuctionsProgram(string participantId, string username, int startingBalance);
    }
}
