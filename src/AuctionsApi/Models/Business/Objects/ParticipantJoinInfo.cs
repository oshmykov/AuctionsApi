using System.ComponentModel.DataAnnotations;

namespace AuctionsApi.Models.Business.Objects
{
    public class ParticipantJoinInfo
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int Balance { get; set; }
    }
}
