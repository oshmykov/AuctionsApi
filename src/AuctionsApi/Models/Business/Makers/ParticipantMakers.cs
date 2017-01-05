using AuctionsApi.Business.Models.Objects;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Security.Claims;

namespace AuctionsApi.Models.Business.Makers
{
    public class ParticipantMakers
    {
        private static readonly IList<ParticipantInfo> participants = new List<ParticipantInfo> {
            new ParticipantInfo {
                Id = "a-randomly-generated-subject-from-the-identity-provider",
                Balance = 500,
                UserName = "jaontika@hotmail.com"
            },
            new ParticipantInfo {
                Id = "another-randomly-generated-subject-from-the-identity-provider",
                Balance = 700,
                UserName = "pinkyleo@gmail.com"
            },
            new ParticipantInfo {
                Id = "a-leading-bidder-participant",
                Balance = 490,
                UserName = "buble@aol.com"
            }
        };

        public static IEnumerable<Claim> GetUserClaims(string id)
        {
            return participants.Where(p => p.Id.Equals(id)).Select(p => new Claim("sub", p.Id));
        }

        public static ParticipantInfo GetInfo(string id)
        {
            return participants.Where(p => p.Id.Equals(id)).SingleOrDefault();
        }
    }
}
