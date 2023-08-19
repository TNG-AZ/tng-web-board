using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public class Signature
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        public string EventId { get; set; }
        public string LegalName { get; set; }
        public string SceneName { get; set; }

        public DateTime SignatureDatetime { get; set; }
        public DateTime CreatedDatetime { get; set; }
        public byte[] SignatureImage { get; set; }
        public byte[] SignedForm { get; set; }

        public virtual Member Member { get; set; }

    }
}
