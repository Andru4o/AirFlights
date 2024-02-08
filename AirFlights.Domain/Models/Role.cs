using System.ComponentModel.DataAnnotations.Schema;

namespace AirFlights.Domain;

public class Role
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Code { get; set; }
    public List<User> Users { get; set; } = new();
}