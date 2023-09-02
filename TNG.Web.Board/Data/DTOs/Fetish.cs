#nullable disable

using System.ComponentModel.DataAnnotations;

namespace TNG.Web.Board.Data.DTOs
{
    public class Fetish
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual IEnumerable<MemberFetish> MemberFetishes { get; set; }
    }
}
