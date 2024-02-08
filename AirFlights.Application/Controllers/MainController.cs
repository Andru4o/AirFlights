using System.Text;
using AirFlights.Domain;
using AirFlights.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Serilog;

namespace AirFlights.Application;

[Route("api/[controller]")]
[ApiController]
public class MainController : ControllerBase
{
    private const string FlightsCacheKey = "flightsKey";
    private const string InvalidUsernameMessage = "Invalid username or password.";
    private const string OriginIsEmptyMessage = "Origin shouldn't be empty";
    private const string UnauthorizedMessage = "Unauthrized";
    private const string IncorrectRoleMessage = "Access is not granted for this method";
    private FlightsDbContext _flightsDbContext;
    private IMemoryCache _cache;
    private Serilog.ILogger _logger;
    
    public MainController(FlightsDbContext flightsDbContext, IMemoryCache cache)
    {
        _flightsDbContext = flightsDbContext;
        _cache = cache;
        _logger = new LoggerConfiguration().WriteTo.File("log.txt",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

    }
    
    /// <summary>
    /// Метод авторизации
    /// </summary>
    /// <param name="authData">Объект для авторизации</param>
    /// <returns></returns>
    [HttpPost]
    [Route("Authorize")]
    public async Task<IActionResult> Authorize(AuthModel authData)
    {
        var validator = new AuthModelValidator();
        var validationInfo = await validator.ValidateAsync(authData);
        if (!validationInfo.IsValid)
        {
            var validationErrors = string.Join(Environment.NewLine, validationInfo.Errors.Select(z => z.ErrorMessage));
            _logger.Error($"Request is not valid. Request: {JsonConvert.SerializeObject(authData)}{Environment.NewLine}" +
                          $"Errors: {validationErrors}");
            return BadRequest(validationErrors);
        }

        var user = _flightsDbContext.Users.FirstOrDefault(x => x.Username == authData.Username && x.Password == authData.Password);

        if (user == null)
        {
            _logger.Error(InvalidUsernameMessage);
            return BadRequest(new { errorText = InvalidUsernameMessage });
        }

        var tokenString = $"{user.Username}_{user.RoleId}_{DateTime.Now.Ticks}";
        
        var response = new
        {
            access_token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenString))
        };
 
        return new JsonResult(response);
    }
    
    /// <summary>
    /// Метод для получения списка рейсов
    /// </summary>
    /// <param name="token">Токен доступа</param>
    /// <param name="origin">Пункт отправления</param>
    /// <param name="destination">Пункт прибытия</param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetFlights")]
    public async Task<IActionResult> GetFlights([FromHeader] string token, string origin, string? destination)
    {
        var userInfo = CheckToken(token);
        if (userInfo == null)
        {
            _logger.Error(UnauthorizedMessage);
            return Unauthorized(UnauthorizedMessage);
        }
        
        if (string.IsNullOrEmpty(origin))
        {
            _logger.Error(OriginIsEmptyMessage);
            return BadRequest(OriginIsEmptyMessage);
            
        }
        
        var flights = await TryGetAndUpdateFlights(origin);
        // если данные не найдены в кэше
        if (flights != null)
        {
            flights = flights.Where(z => z.Origin == origin).ToList();
            if (!string.IsNullOrEmpty(destination))
                flights = flights.Where(z => z.Destination == destination).ToList();
        }
        return new JsonResult(flights);
    }
    
    /// <summary>
    /// Метод для добавления нового рейса
    /// </summary>
    /// <param name="token">Токен доступа</param>
    /// <param name="flight">Информация о рейсе</param>
    /// <returns></returns>
    [HttpPost]
    [Route("AddFlight")]
    public async Task<IActionResult> AddFlight([FromHeader] string token, Flight flight)
    {
        var userInfo = CheckToken(token);
        if (userInfo == null)
        {
            _logger.Error(UnauthorizedMessage);
            return Unauthorized(UnauthorizedMessage);
        }

        if (userInfo.RoleId != 2)
        {
            return Unauthorized(IncorrectRoleMessage);
        }

        var validator = new FlightValidator();
        var validationInfo = await validator.ValidateAsync(flight);
        if (!validationInfo.IsValid)
        {
            var validationErrors = string.Join(Environment.NewLine, validationInfo.Errors.Select(z => z.ErrorMessage));
            _logger.Error($"UserName: {userInfo.Username}. Request is not valid. Request: {JsonConvert.SerializeObject(flight)}{Environment.NewLine}" +
                          $"Errors: {validationErrors}");
            return BadRequest(validationErrors);
        }

        try
        {
            await _flightsDbContext.AddAsync(flight);
            await _flightsDbContext.SaveChangesAsync();
            await TryGetAndUpdateFlights(flight.Origin);
        }
        catch (Exception e)
        {
            _logger.Error($"UserName: {userInfo.Username}. Exception {e.Message} {Environment.NewLine} {Environment.StackTrace}.");
        }
        return Ok();
    }
    
    /// <summary>
    /// Метод для изменения статуса рейса
    /// </summary>
    /// <param name="token">Токен доступа</param>
    /// <param name="flightId">Идентификатор рейса</param>
    /// <param name="status">Статус рейса</param>
    /// <returns></returns>
    [HttpPut]
    [Route("EditFlight/{flightId:int}")]
    public async Task<IActionResult> EditFlight([FromHeader] string token, int flightId, StatusEnum status)
    {
        var userInfo = CheckToken(token);
        if (userInfo == null)
        {
            _logger.Error(UnauthorizedMessage);
            return Unauthorized(UnauthorizedMessage);
        }

        if (userInfo.RoleId != 2)
        {
            return Unauthorized(IncorrectRoleMessage);
        }

        var flight = await _flightsDbContext.Flights.FirstOrDefaultAsync(z => z.Id == flightId);
        if (flight != null)
        {
            try
            {
                flight.Status = status;
                await _flightsDbContext.SaveChangesAsync();
                await TryGetAndUpdateFlights(flight.Origin);
            }
            catch (Exception e)
            {
                _logger.Error(
                    $"UserName: {userInfo.Username}. Exception {e.Message} {Environment.NewLine} {Environment.StackTrace}.");
            }
        }
        else
        {
            _logger.Error($"UserName: {userInfo.Username}. Flight is not found by Id {flightId}.");
            return NotFound();
        }

        return Ok();
    }

    private async Task<List<Flight>?> TryGetAndUpdateFlights(string origin)
    {
        _cache.TryGetValue($"{FlightsCacheKey}_{origin}", out List<Flight>? flights);
        if (flights == null)
        {
            // обращаемся к базе данных
            flights = await _flightsDbContext.Flights.Where(z => z.Origin == origin).ToListAsync();
            if (flights != null)
            {
                _cache.Set($"{FlightsCacheKey}_{origin}", flights, 
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
        }

        return flights;
    }

    private User CheckToken(string token)
    {
        try
        {
            var decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var splittedData = decodedData.Split('_');
            return new User()
            {
                Username = splittedData[0],
                RoleId = int.Parse(splittedData[1])
            };
        }
        catch (Exception e)
        {
            return null;
        }
    }
}