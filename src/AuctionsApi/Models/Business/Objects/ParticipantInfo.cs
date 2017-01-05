using System.ComponentModel.DataAnnotations;

namespace AuctionsApi.Business.Models.Objects
{
    public class ParticipantInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        [Required]
        public int Balance { get; set; }
    }
}
