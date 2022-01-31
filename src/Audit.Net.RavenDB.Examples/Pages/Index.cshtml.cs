using Audit.Core;
using Audit.Net.RavenDB.Examples.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Raven.Client.Documents;
using System.Text.Json.Serialization;

namespace Audit.Net.RavenDB.Examples.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDocumentStore _store;

        public IndexModel(ILogger<IndexModel> logger, IDocumentStore store)
        {
            _logger = logger;
            _store = store;
        }

        public async Task OnGet()
        {
            using (var session = _store.OpenAsyncSession())
            {
                PageView? entity = null;
                using (var audit = await AuditScope.CreateAsync("PageView:Create", () => entity))
                {
                    entity = new PageView
                    {
                        Name = "Index"
                    };
                    await session.StoreAsync(entity);
                    await session.SaveChangesAsync();
                    audit.SetCustomField("Id", entity.Id);
                }
            }
        }
    }

    public class Test
    {

        [JsonExtensionData]
        public string Id { get; set; }
    }
}