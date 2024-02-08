using System.ComponentModel.DataAnnotations.Schema;

namespace AirFlights.Domain;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int RoleId { get; set; }
    [ForeignKey("RoleId")]
    public Role Role { get; set; }
}