using System;

namespace BancDelTemps.ApiRest.Models
{
    public class UserPermiso
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int PermisoId { get; set; }
        public Permiso Permiso { get; set; }
        public int GrantedById { get; set; }
        public User GrantedBy { get; set; }
        public DateTime GrantedDate { get; set; }
        public int? RevokedById { get; set; }
        public User RevokedBy { get; set; }
        public DateTime? RevokedDate { get; set; }

        public bool IsActive => !RevokedById.HasValue || GrantedDate > RevokedDate.Value;
    }
}
