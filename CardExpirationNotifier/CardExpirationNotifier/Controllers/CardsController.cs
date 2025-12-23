using CardExpirationNotifier.BusinessLogic.Services;
using CardExpirationNotifier.Models.Requests;
using CardExpirationNotifier.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CardExpirationNotifier.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly ILogger<CardsController> _logger;

    public CardsController(ICardService cardService, ILogger<CardsController> logger)
    {
        _cardService = cardService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddCard([FromBody] AddCardRequest request)
    {
        try
        {
            var card = await _cardService.AddCardAsync(
                request.CardMask,
                request.ExpirationYear,
                request.ExpirationMonth,
                request.CardType,
                request.UserFirstName,
                request.UserLastName
            );

            var response = new CardResponse
            {
                Id = card.Id,
                CardMask = card.CardMask,
                ExpirationYear = card.ExpirationYear,
                ExpirationMonth = card.ExpirationMonth,
                CardType = card.CardType,
                UserFirstName = card.UserFirstName,
                UserLastName = card.UserLastName,
                NotificationSent = card.NotificationSent,
                CreatedAt = card.CreatedAt
            };

            return CreatedAtAction(nameof(AddCard), response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid card data provided");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding card");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCards(
        [FromQuery] int startYear,
        [FromQuery] int startMonth,
        [FromQuery] int endYear,
        [FromQuery] int endMonth,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50)
    {
        try
        {
            var cards = await _cardService.GetCardsByDateRangeAsync(startYear, startMonth, endYear, endMonth, offset, limit);

            var response = cards.Select(c => new CardResponse
            {
                Id = c.Id,
                CardMask = c.CardMask,
                ExpirationYear = c.ExpirationYear,
                ExpirationMonth = c.ExpirationMonth,
                CardType = c.CardType,
                UserFirstName = c.UserFirstName,
                UserLastName = c.UserLastName,
                NotificationSent = c.NotificationSent,
                CreatedAt = c.CreatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cards");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("pending-notification")]
    public async Task<IActionResult> GetPendingNotifications(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50)
    {
        try
        {
            var cards = await _cardService.GetPendingNotificationsAsync(offset, limit);

            var response = cards.Select(c => new CardResponse
            {
                Id = c.Id,
                CardMask = c.CardMask,
                ExpirationYear = c.ExpirationYear,
                ExpirationMonth = c.ExpirationMonth,
                CardType = c.CardType,
                UserFirstName = c.UserFirstName,
                UserLastName = c.UserLastName,
                NotificationSent = c.NotificationSent,
                CreatedAt = c.CreatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending notifications");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
