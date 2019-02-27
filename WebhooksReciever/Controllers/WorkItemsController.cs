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


namespace WebhooksReceiver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkItems : ControllerBase
    {
        IWorkItemRepo _workItemRepo ; 

        public WorkItems(IWorkItemRepo workItemRepo)
        {
            _workItemRepo = workItemRepo;
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] JObject payload)
        {
            PayloadViewModel vm = BuildPayloadViewModel(payload);

            if (vm.eventType != "workitem.created")
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

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.AssignedTo",
                    Value = vm.createdBy
                }
            );

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

            var result = _workItemRepo.UpdateWorkItem(patchDocument, vm.id);           

            return (result != null) ? new OkResult() : new StatusCodeResult(500);             

        }

        private PayloadViewModel BuildPayloadViewModel(JObject body)
        {
            PayloadViewModel vm = new PayloadViewModel();

            string url = body["resource"]["url"].ToString();
            url = url.Replace("http://", string.Empty);
            string[] split = url.Split('/');

            vm.id = Convert.ToInt32(body["resource"]["id"].ToString());
            vm.eventType = body["eventType"].ToString();
            vm.rev = Convert.ToInt32(body["resource"]["rev"].ToString());
            vm.url = body["resource"]["url"].ToString();
            vm.organization = split[0].ToString();
            vm.teamProject = body["resource"]["fields"]["System.AreaPath"].ToString();
            vm.createdBy = body["resource"]["fields"]["System.CreatedBy"]["displayName"].ToString();

            return vm;
        }
    }
}
