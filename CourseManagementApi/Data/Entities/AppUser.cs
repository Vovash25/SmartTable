using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManagementApi.Data.Entities
{
    // Ролі: "Admin" (звичайний адмін) або "SuperAdmin" (бачить Audit Logs
    // і може створювати/видаляти інших адмінів).
    [Table("app_users")]
    public class AppUser
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column("password_salt")]
        public string PasswordSalt { get; set; } = string.Empty;

        [Required]
        [Column("role")]
        public string Role { get; set; } = "Admin";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
