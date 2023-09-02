#nullable disable

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace TNG.Web.Board.Data.DTOs
{
    [Index(nameof(Name), IsUnique = true)]
    public class Tag
    {
        [Key]
        public Guid Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

#nullable enable
        public virtual IEnumerable<NoteTag>? NoteTags { get; set; }
#nullable disable
    }
}
