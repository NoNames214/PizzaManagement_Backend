using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaManagement.model
{
    public class User : BaseEntity
    {
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string ? UserName { get; set; }     
        public string ? Email { get; set; }
        public string ? Address { get; set; }
        public string ? AvatarUrl { get; set; }
        public string ? FullName { get; set; }
        public string ? Role { get; set; }
        public string ? PasswordHash { get; set; }
        public bool isLocked { get; set; } = false;
    }
}
 