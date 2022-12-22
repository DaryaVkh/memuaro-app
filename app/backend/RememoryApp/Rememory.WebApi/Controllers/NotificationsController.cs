using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rememory.Auth;
using Rememory.Email;
using Rememory.Persistance.Entities;
using Rememory.Persistance.Repositories.NotificationSettingsRepository;
using Rememory.WebApi.Dtos.Notifications;
using Rememory.WebApi.Exceptions;

namespace Rememory.WebApi.Controllers;

[Authorize]
public class NotificationsController : BaseController
{
    private readonly INotificationSettingsRepository _notificationSettingsRepository;
    private readonly IEmailClient _emailClient;
    
    public NotificationsController(
        AuthProvider authProvider,
        INotificationSettingsRepository notificationSettingsRepository,
        IEmailClient emailClient) : base(authProvider)
    {
        _notificationSettingsRepository = notificationSettingsRepository;
        _emailClient = emailClient;
    }

    [HttpPost]
    [Route("settings/{id:guid}")]
    public async Task<ActionResult<NotificationSettingsDto>> SaveNotificationSettings(Guid id, [FromBody] NotificationSettingsDto request)
    {
        CheckAccessForUser(id);

        var notificationSettings = new NotificationSettings
        {
            Id = id,
            Email = request.Email,
            TelegramName = request.TelegramName,
            PeriodInDays = request.PeriodInDays
        };

        await _notificationSettingsRepository.CreateOrUpdate(notificationSettings);
        var message =
            $"<div>Здравствуйте, {notificationSettings.Email}! Теперь вы будете получать уведомления на эту почту с периодичностью в {notificationSettings.PeriodInDays} дней.</div>" +
            "<div>Если вы хотите перестать получать уведомления, то перейдите на <a>https://app.rememory.ru</a> и отключите их.</div>";

        await _emailClient.SendMessage(notificationSettings.Email!, message);

        return Ok(new NotificationSettingsDto(notificationSettings));
    }

    [HttpGet]
    [Route("settings/{id:guid}")]
    public async Task<ActionResult<NotificationSettingsDto>> GetNotificationSettings(Guid id)
    {
        CheckAccessForUser(id);

        var settings = await _notificationSettingsRepository.GetAsync(id);
        
        return Ok(settings != null ? new NotificationSettingsDto(settings) : null);
    }
    
    private void CheckAccessForUser(Guid userId)
    {
        var accessToken = _authProvider.ParseAuthHeader(HttpContext.Request.Headers.Authorization);
        var userCred = _authProvider.GetCurrentUserCredential(accessToken);
        if (userCred.Id != userId) throw new ForbiddenException();
    }
}