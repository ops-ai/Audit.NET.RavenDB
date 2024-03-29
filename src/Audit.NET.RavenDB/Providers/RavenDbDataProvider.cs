﻿using Audit.Core;
using Audit.NET.RavenDB.Providers;
using Force.DeepCloner;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Raven.Client.Documents;
using SimpleHelpers;

namespace Audit.NET.RavenDB
{
    public class RavenDbDataProvider : AuditDataProvider, IAuditQueryProvider
    {
        private readonly IDocumentStore _store;

        /// <summary>
        /// Gets or sets the RavenDB Database name.
        /// </summary>
        public string Database { get; set; } = "Audit";

        /// <summary>
        /// Gets or sets a value to indicate whether the element names should be validated/fixed or not.
        /// If <c>true</c> the element names are not validated, use this when you know the element names will not contain invalid characters.
        /// If <c>false</c> (default) the element names are validated and fixed to avoid containing invalid characters.
        /// </summary>
        public bool IgnoreElementNames { get; set; } = false;

        public bool StoreDiffOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets the default JsonSerializerSettings.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new JavaScriptDateTimeConverter() } };

        public RavenDbDataProvider(IDocumentStore store, string? databaseName, JsonSerializerSettings? jsonSerializerSettings = null, bool? storeDiffOnly = false)
        {
            _store = store;
            if (databaseName != null)
                Database = databaseName;
            if (jsonSerializerSettings != null)
                JsonSerializerSettings = jsonSerializerSettings;
            if (storeDiffOnly.HasValue)
                StoreDiffOnly = storeDiffOnly.Value;
        }

        public override object? Serialize<T>(T value)
        {
            if (value == null)
                return null;

            return value.DeepClone();// JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, JsonSerializerSettings), value.GetType(), JsonSerializerSettings);
        }

        public override object InsertEvent(AuditEvent auditEvent)
        {
            using (var session = _store.OpenSession(Database))
            {
                if (StoreDiffOnly)
                {
                    if (auditEvent.Target != null)
                    {
                        var diff = ObjectDiffPatch.GenerateDiff(auditEvent.Target.Old, auditEvent.Target.New);

                        auditEvent.Target.Old = diff.OldValues;
                        auditEvent.Target.New = diff.NewValues;
                    }
                }

                session.Store(auditEvent);
                session.SaveChanges();

                return session.Advanced.GetDocumentId(auditEvent);
            }
        }

        /// <summary>
        /// Insert an event to the data source returning the event id generated
        /// </summary>
        /// <param name="auditEvent">The audit event being inserted.</param>
        /// <returns></returns>
        public override async Task<object> InsertEventAsync(AuditEvent auditEvent)
        {
            using (var session = _store.OpenAsyncSession(Database))
            {
                if (StoreDiffOnly)
                {
                    try
                    {
                        if (auditEvent.Target != null)
                        {
                            var diff = ObjectDiffPatch.GenerateDiff(auditEvent.Target.Old, auditEvent.Target.New);

                            auditEvent.Target.Old = diff.OldValues;
                            auditEvent.Target.New = diff.NewValues;
                        }
                    }
                    catch { }
                }

                await session.StoreAsync(auditEvent);
                await session.SaveChangesAsync();

                return session.Advanced.GetDocumentId(auditEvent);
            }
        }

        /// <summary>
        /// Retrieves a saved audit event from its id. Override this method to provide a way to access the audit events by id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventId">The event id being retrieved.</param>
        /// <returns></returns>
        public override T GetEvent<T>(object eventId)
        {
            using (var session = _store.OpenSession())
            {
                var auditEvent = session.Load<T>(eventId.ToString());
                return (T)auditEvent;
            }
        }

        /// <summary>
        /// Asychronously retrieves a saved audit event from its id. Override this method
        /// to provide a way to access the audit events by id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventId">The event id being retrieved.</param>
        /// <returns></returns>
        public override async Task<T> GetEventAsync<T>(object eventId)
        {
            using (var session = _store.OpenAsyncSession())
            {
                var auditEvent = await session.LoadAsync<T>(eventId.ToString());
                return (T)auditEvent;
            }
        }

        /// <summary>
        /// Saves the specified audit event. Triggered when the scope is saved. Override
        /// this method to replace the specified audit event on the data source.
        /// </summary>
        /// <param name="eventId">The event id being replaced.</param>
        /// <param name="auditEvent">The audit event.</param>
        public override void ReplaceEvent(object eventId, AuditEvent auditEvent)
        {
            using (var session = _store.OpenSession())
            {
                session.Store(auditEvent, eventId.ToString());
                session.SaveChanges();
            }
        }

        /// <summary>
        /// Saves the specified audit event. Triggered when the scope is saved. Override
        /// this method to replace the specified audit event on the data source.
        /// </summary>
        /// <param name="eventId">The event id being replaced.</param>
        /// <param name="auditEvent">The audit event.</param>
        /// <returns></returns>
        public override async Task ReplaceEventAsync(object eventId, AuditEvent auditEvent)
        {
            using (var session = _store.OpenAsyncSession())
            {
                await session.StoreAsync(auditEvent, eventId.ToString());
                await session.SaveChangesAsync();
            }
        }

        public async Task<List<AuditEvent>> GetAuditEventsAsync(object id, string eventType, CancellationToken ct = default)
        {
            using (var session = _store.OpenAsyncSession())
            {
                return await session.Advanced.AsyncDocumentQuery<AuditEvent>()
                            .OpenSubclause()
                                .WhereEquals("Id", id)
                                .OrElse().WhereEquals("CustomFields.Id", id)
                            .CloseSubclause()
                            .AndAlso().WhereStartsWith("EventType", eventType).ToListAsync(ct);
            }
        }

        public List<AuditEvent> GetAuditEvents(object id, string eventType)
        {
            using (var session = _store.OpenSession())
            {
                return session.Advanced.DocumentQuery<AuditEvent>()
                            .OpenSubclause()
                                .WhereEquals("Id", id)
                                .OrElse().WhereEquals("CustomFields.Id", id)
                            .CloseSubclause()
                            .AndAlso().WhereStartsWith("EventType", eventType).ToList();
            }
        }
    }
}
