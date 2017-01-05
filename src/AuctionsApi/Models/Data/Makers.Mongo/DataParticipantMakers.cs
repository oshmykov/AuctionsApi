using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuctionsApi.Models.Data.Makers.Mongo
{
    public class DataParticipantMakers
    {
        private static readonly IList<ParticipantDoc> participants = new List<ParticipantDoc> {
            new ParticipantDoc {
                Id = "a-randomly-generated-subject-from-the-identity-provider",
                Balance = 500,
                UserName = "jaontika@hotmail.com",
                MyAuctions = new Dictionary<string, int>()
            },
            new ParticipantDoc {
                Id = "another-randomly-generated-subject-from-the-identity-provider",
                Balance = 700,
                UserName = "pinkyleo@gmail.com",
                MyAuctions = new Dictionary<string, int>()
            },
            new ParticipantDoc {
                Id = "a-leading-bidder-participant",
                Balance = 490,
                UserName = "buble@aol.com",
                MyAuctions = new Dictionary<string, int>
                {
                    { "random-guid-2", 10 }
                }
            },
            new ParticipantDoc {
                Id = "an-outbid-participant",
                Balance = 50,
                UserName = "lourens@ya.com",
                MyAuctions = new Dictionary<string, int>
                {
                    { "random-guid-2", 0 }
                }
            },
            new ParticipantDoc {
                Id = "did-not-win-participant",
                Balance = 500,
                UserName = "heywai@tomcat.net",
                MyAuctions = new Dictionary<string, int>
                {
                    { "random-guid-3", 0 }
                }
            },
            new ParticipantDoc {
                Id = "a-winner-participant",
                Balance = 100,
                UserName = "heywai@tomcat.net",
                MyAuctions = new Dictionary<string, int>
                {
                    { "random-guid-3", 250 }
                }
            },
            new ParticipantDoc {
                Id = "invalid-balance-participant",
                Balance = -10,
                UserName = "kasperskiy@hotmail.com",
                MyAuctions = new Dictionary<string, int>()
            },
        };

        public static ParticipantDoc GetParticipant(string id)
        {
            return participants.Where(p => p.Id.Equals(id)).SingleOrDefault();
        }

        public static ParticipantDoc GetParticipant(Func<ParticipantDoc, bool> filter)
        {
            return participants.Where(filter).FirstOrDefault();
        }
    }
}
