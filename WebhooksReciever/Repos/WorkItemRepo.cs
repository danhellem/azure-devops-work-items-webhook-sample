using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebhooksReceiver.Models;
using WebhooksReceiver.ViewModels;

namespace WebhooksReceiver.Repos
{
    public class WorkItemRepo : IWorkItemRepo
    {
        private IOptions<AppSettings> _appSettings;

        public WorkItemRepo(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        public WorkItem UpdateWorkItem(JsonPatchDocument patchDocument, PayloadViewModel vm)
        {
            string pat = vm.pat;          
            Uri baseUri = new Uri("https://dev.azure.com/" + vm.organization);

            VssCredentials clientCredentials = new VssCredentials(new VssBasicCredential("username", pat));
            VssConnection connection = new VssConnection(baseUri, clientCredentials);

            WorkItemTrackingHttpClient client = connection.GetClient<WorkItemTrackingHttpClient>();
            WorkItem result = null;

            try
            {
                result = client.UpdateWorkItemAsync(patchDocument, vm.id).Result;
            }
            catch (Exception ex)
            {
                result = null;
            }
            finally
            {
                clientCredentials = null;
                connection = null;
                client = null;
            }

           return result;
        }
    }

    public interface IWorkItemRepo
    {
        WorkItem UpdateWorkItem(JsonPatchDocument patchDocument, PayloadViewModel vm);
    }
}
