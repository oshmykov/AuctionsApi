using AuctionsApi.Business.Models.Abstract;
using AuctionsApi.Business.Models.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuctionsApi.Models.Data.Abstract;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;

namespace AuctionsApi.Business.Models.Impl.Mongo
{
    public class ParticipantsMongoService : IParticipantsService
    {
        private const string HAS_JOINED_ALREADY = "Participant has joined the Program already";
        private const string INVALID_FUNDS = "Invalid balance to join the Program";

        private readonly IRepository<ParticipantDoc> participantsRepository;

        public ParticipantsMongoService(IRepository<ParticipantDoc> participantsRepository)
        {
            this.participantsRepository = participantsRepository;
        }

        public async Task<ParticipantInfo> GetParticipantInfo(string participantId)
        {
            var participant = await participantsRepository.ReadOneAsync(p => p.Id.Equals(participantId));

            if (participant != null)
            {
                return new ParticipantInfo
                {
                    Id = participant.Id,
                    Balance = participant.Balance,
                    UserName = participant.UserName
                };
            }

            return null;
        }

        public async Task<IPlainCommandResult> JoinAuctionsProgram(string participantId, string username, int startingBalance)
        {
            var participant = await participantsRepository.ReadOneAsync(p => p.Id.Equals(participantId));
            
            if (participant != null)
            {
                return CommandResult.BadRequest(HAS_JOINED_ALREADY);
            }

            if (startingBalance < 0)
            {
                return CommandResult.BadRequest(INVALID_FUNDS);
            }

            participantsRepository.Create(new ParticipantDoc
            {
                Id = participantId,
                MyAuctions = new Dictionary<string, int>(),
                UserName = username,
                Balance = startingBalance
            });

            try
            {
                await participantsRepository.SaveChangesAsync();

                return CommandResult.Success();
            }
            catch (Exception ex)
            {
                return CommandResult.BadRequest(ex.Message);
            }
        }
    }
}
