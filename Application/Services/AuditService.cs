using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<AuditService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task LogActionAsync(string entity, long? entityId, string action, string? details = null)
        {
            try
            {
                AuditAction auditAction;
                if (!Enum.TryParse(action.ToUpper(), out auditAction))
                {
                    _logger.LogWarning("Invalid audit action: {Action}", action);
                    return;
                }

                var audit = new Audit
                {
                    User = _currentUserService.UserId,
                    Entity = entity,
                    EntityId = entityId,
                    Action = auditAction,
                    When = DateTime.UtcNow,
                    Ip = _currentUserService.IpAddress ?? "Unknown",
                    Details = details
                };

                _unitOfWork.Audits.Add(audit);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Audit log created: {User} performed {Action} on {Entity} {EntityId}",
                    _currentUserService.UserId, action, entity, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log for {User} {Action} {Entity} {EntityId}",
                    _currentUserService.UserId, action, entity, entityId);
            }
        }
    }
}