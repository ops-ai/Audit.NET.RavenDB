using Audit.Core;

namespace Audit.NET.RavenDB.Providers
{
    public interface IAuditQueryProvider
    {
        Task<List<AuditEvent>> GetAuditEventsAsync(object id, string eventType, CancellationToken ct = default);

        List<AuditEvent> GetAuditEvents(object id, string eventType);
    }
}
