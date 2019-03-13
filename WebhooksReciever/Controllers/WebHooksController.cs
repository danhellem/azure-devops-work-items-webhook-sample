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
using Microsoft.Extensions.Primitives;
using WebhooksReceiver.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace WebhooksReceiver.Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    public class WebHooks : ControllerBase
    {
        IWorkItemRepo _workItemRepo ;
        IOptions<AppSettings> _appSettings;

        public WebHooks(IWorkItemRepo workItemRepo, IOptions<AppSettings> appSettings)
        {
            _workItemRepo = workItemRepo;
            _appSettings = appSettings;
        }

        // POST api/values
        [HttpPost]
        [Route("workitem/new")]
        public IActionResult Post([FromBody] JObject payload)
        {          
            string tags = Request.Headers.ContainsKey("Work-Item-Tags") ? Request.Headers["Work-Item-Tags"] : new StringValues("");
            string authHeader = Request.Headers.ContainsKey("Authorization") ? Request.Headers["Authorization"] : new StringValues("");

            if (! authHeader.StartsWith("Basic"))
            {
                return new StandardResponseObjectResult("missing basic authorization header", StatusCodes.Status401Unauthorized);
            }

            //get pat from basic authorization header. This was set in the web hook
            string pat = this.GetPersonalAccessToken(authHeader);           

            PayloadViewModel vm = this.BuildPayloadViewModel(payload);

            //make sure pat is not empty, if it is, pull from appsettings
            vm.pat = (! string.IsNullOrEmpty(pat)) ? pat : _appSettings.Value.AzureDevOpsToken;

            if (string.IsNullOrEmpty(vm.pat))
            {
                return new StandardResponseObjectResult("missing pat from authorization header and appsettings", StatusCodes.Status404NotFound);
            }

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

            if (! string.IsNullOrEmpty(tags))
            { 
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                       Operation = Operation.Add,
                       Path = "/fields/System.Tags",
                       Value = tags
                    }
                );
            }

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.IterationPath",
                    Value = vm.teamProject
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


        /// <summary>
        /// get personal access token where the token is the password in the basic authorization header
        /// </summary>
        /// <param name="basicAuthHeader"></param>
        /// <returns>personal access token</returns>
        private string GetPersonalAccessToken(string basicAuthHeader)
        {
            basicAuthHeader = basicAuthHeader.Replace("Basic ", "");
            string pat = string.Empty;

            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string userNameAndPat = encoding.GetString(Convert.FromBase64String(basicAuthHeader));
            
            //pull pat from authorization header
            if (!string.IsNullOrEmpty(basicAuthHeader))
            {
                string[] split = userNameAndPat.Split(':');
                pat = split.Count() == 2 ? split[1].ToString() : string.Empty;
            }

            return pat;
        }
    }
}
