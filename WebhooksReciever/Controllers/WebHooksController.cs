using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;

using WebhooksReceiver.ViewModels;
using WebhooksReceiver.Repos;
using Microsoft.TeamFoundation.Common;

namespace WebhooksReceiver.Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    public class WebHooks : ControllerBase
    {
        IWorkItemRepo _workItemRepo ; 

        public WebHooks(IWorkItemRepo workItemRepo)
        {
            _workItemRepo = workItemRepo;
        }

        // POST api/values
        [HttpPost]
        [Route("workitem/new")]
        public IActionResult Post([FromBody] JObject payload)
        {
            PayloadViewModel vm = BuildPayloadViewModel(payload);

            if (vm.eventType != "workitem.created")
            {
                return new OkResult();
            }

            if (vm.id == -1)
            {
                return new OkResult();
            }

            JsonPatchDocument patchDocument = new JsonPatchDocument();

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Test,
                    Path = "/rev",
                    Value = (vm.rev + 1).ToString()                    
                }
            );

            if (string.IsNullOrEmpty(vm.assignedTo))
            {            
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.AssignedTo",
                        Value = vm.createdBy
                    }
                );
            }

            patchDocument.Add(
                new JsonPatchOperation()
                {
                   Operation = Operation.Add,
                   Path = "/fields/System.Tags",
                   Value = "Work"
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.IterationPath",
                    Value = vm.teamProject
                }
            );

            var result = _workItemRepo.UpdateWorkItem(patchDocument, vm);           

            return (result != null) ? new OkResult() : new StatusCodeResult(500);             

        }

        private PayloadViewModel BuildPayloadViewModel(JObject body)
        {
            PayloadViewModel vm = new PayloadViewModel();

            string url = body["resource"]["url"] == null ? null : body["resource"]["url"].ToString();
            string org = GetOrganization(url);

            vm.id = body["resource"]["id"] == null ? - 1 : Convert.ToInt32(body["resource"]["id"].ToString());
            vm.eventType = body["eventType"] == null ? null : body["eventType"].ToString();
            vm.rev = body["resource"]["rev"] == null ? -1 : Convert.ToInt32(body["resource"]["rev"].ToString());
            vm.url = body["resource"]["url"] == null ? null : body["resource"]["url"].ToString();
            vm.organization = org;
            vm.teamProject = body["resource"]["fields"]["System.AreaPath"] == null ? null : body["resource"]["fields"]["System.AreaPath"].ToString();
            vm.createdBy = body["resource"]["fields"]["System.CreatedBy"]["displayName"] == null ? null : body["resource"]["fields"]["System.CreatedBy"]["displayName"].ToString();
            vm.assignedTo = body["resource"]["fields"]["System.AssignedTo"] == null ? null : body["resource"]["fields"]["System.Assigned"].ToString();

            return vm;
        }

        private string GetOrganization(string url)
        {
            url = url.Replace("http://", string.Empty);
            url = url.Replace("https://", string.Empty);

            if (url.Contains(value:"visualstudio.com"))
            {
                string[] split = url.Split('.');
                return split[0].ToString();
            }  
            
            if (url.Contains("dev.azure.com"))
            {
                url = url.Replace("dev.azure.com/", string.Empty);
                string[] split = url.Split('/');
                return split[0].ToString();
            }

            return string.Empty;
        }
    }
}
