using System.ComponentModel.DataAnnotations;

namespace AirFlights.Domain;

/// <summary>
/// Объект для авторизации
/// </summary>
public class AuthModel
{
    /// <summary>
    /// Логин
    /// </summary>
    [Required]
    public string Username { get; set; }
    /// <summary>
    /// Хэш пароля
    /// </summary>
    [Required]
    public string Password { get; set; }
}