using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirFlights.Domain;

/// <summary>
/// Информация о рейсе
/// </summary>
public class Flight
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    /// <summary>
    /// Аэропорт отправления
    /// </summary>
    [Required]
    public string Origin { get; set; }
    /// <summary>
    /// Аэропорт прибытия
    /// </summary>
    [Required]
    public string Destination { get; set; }
    /// <summary>
    /// Дата вылета
    /// </summary>
    [Required]
    public DateTimeOffset Departure { get; set; }
    /// <summary>
    /// Дата прилёта
    /// </summary>
    public DateTimeOffset? Arrival { get; set; }
    /// <summary>
    /// Статус рейса
    /// </summary>
    [Required]
    public StatusEnum Status { get; set; }
}