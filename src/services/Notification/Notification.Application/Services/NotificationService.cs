using GaniPay.Notification.Application.Abstractions;
using GaniPay.Notification.Application.Abstractions.Providers;
using GaniPay.Notification.Application.Abstractions.Repositories;
using GaniPay.Notification.Application.Contracts.Dtos;
using GaniPay.Notification.Application.Contracts.Requests;
using GaniPay.Notification.Domain.Entities;
using GaniPay.Notification.Domain.Enums;

namespace GaniPay.Notification.Application.Services;

public sealed class NotificationService : INotificationService
{
    // DB CHECK constraint uyumlu set: sms / email / inapp / push
    private static readonly HashSet<string> AllowedChannels =
        new(StringComparer.OrdinalIgnoreCase) { "sms", "email", "inapp", "push" };

    private readonly INotificationLogRepository _repo;
    private readonly INotificationProvider _provider;
    private readonly IUnitOfWork _uow;

    public NotificationService(
        INotificationLogRepository repo,
        INotificationProvider provider,
        IUnitOfWork uow)
    {
        _repo = repo;
        _provider = provider;
        _uow = uow;
    }

    public async Task<SendNotificationResultDto> SendAsync(SendNotificationRequest request, CancellationToken ct)
    {
        Validate(request);

        var normalizedChannel = NormalizeChannel(request.Channel);

        // ✅ LOG OLUŞTURMA KISMI BURASI (SendAsync içinde)
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Channel = normalizedChannel,              // ✅ DB constraint uyumlu
            TemplateCode = request.TemplateCode.Trim(),
            Payload = request.Payload.Trim(),         // payload boşlukları da temizleyelim
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            SentAt = null,
            ErrorMessage = null
        };

        await _repo.AddAsync(log, ct);
        await _uow.SaveChangesAsync(ct);

        try
        {
            await _provider.SendAsync(log.Channel, log.TemplateCode, log.Payload, ct);

            log.Status = NotificationStatus.Sent;
            log.SentAt = DateTime.UtcNow;
            log.ErrorMessage = null;
        }
        catch (Exception ex)
        {
            log.Status = NotificationStatus.Failed;
            log.SentAt = null;

            // DB'de 500 limitin var, taşarsa kırp.
            var msg = ex.Message ?? "unknown error";
            log.ErrorMessage = msg.Length <= 500 ? msg : msg[..500];
        }

        await _repo.UpdateAsync(log, ct);
        await _uow.SaveChangesAsync(ct);

        return new SendNotificationResultDto(log.Id, log.Status.ToString());
    }

    public async Task<NotificationLogDto> GetAsync(GetNotificationRequest request, CancellationToken ct)
    {
        var log = await _repo.GetByIdAsync(request.Id, ct)
                  ?? throw new InvalidOperationException("notification not found");

        return ToDto(log);
    }

    public async Task<List<NotificationLogDto>> GetCustomerLogsAsync(GetCustomerNotificationsRequest request, CancellationToken ct)
    {
        if (request.CustomerId == Guid.Empty)
            throw new InvalidOperationException("customerId is required");

        var logs = await _repo.GetByCustomerIdAsync(request.CustomerId, ct);
        return logs.Select(ToDto).ToList();
    }

    private static void Validate(SendNotificationRequest request)
    {
        if (request.CustomerId == Guid.Empty)
            throw new InvalidOperationException("customerId is required");

        if (string.IsNullOrWhiteSpace(request.Channel))
            throw new InvalidOperationException("channel is required");

        if (string.IsNullOrWhiteSpace(request.TemplateCode))
            throw new InvalidOperationException("templateCode is required");

        if (string.IsNullOrWhiteSpace(request.Payload))
            throw new InvalidOperationException("payload is required");
    }

    private static string NormalizeChannel(string channel)
    {
        var c = channel.Trim().ToLowerInvariant();

        // alias desteği
        c = c switch
        {
            "mail" => "email",
            "e-mail" => "email",
            "in_app" => "inapp",
            "in-app" => "inapp",
            "app" => "inapp",
            _ => c
        };

        if (!AllowedChannels.Contains(c))
            throw new InvalidOperationException("channel is invalid. allowed: sms, email, inapp, push");

        return c;
    }

    private static NotificationLogDto ToDto(NotificationLog x)
        => new(
            x.Id,
            x.CustomerId,
            x.Channel,
            x.TemplateCode,
            x.Payload,
            x.Status.ToString(),
            x.CreatedAt,
            x.SentAt,
            x.ErrorMessage
        );
}
