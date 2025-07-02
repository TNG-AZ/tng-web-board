namespace TNG.Web.Board.Data.DTOs
{
    public class VolunteerPositionRole
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<VolunteerPositionRole> Roles { get; set; }
    }
}
