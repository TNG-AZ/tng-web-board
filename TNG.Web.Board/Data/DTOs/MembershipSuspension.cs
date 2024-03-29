﻿#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNG.Web.Board.Data.DTOs
{
    public class MembershipSuspension
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [ForeignKey(nameof(Member))]
        public Guid MemberId { get; set; }
        [Required]
        public DateTime StartDate { get; set;} = DateTime.Now;
        public DateTime? EndDate { get; set;}
        public string Reason { get; set; }

        public virtual Member Member { get; set; }
    }
}
