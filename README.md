# azure-devops-work-items-webhook-sample
Simple example of a Webhook receiver from Azure Boards work item create event

## Overview
- Written in .NET Core
- C#
- Hosted as an App Service in Azure 
- REST API endpoint that accepts a request from a work item create event from [Azure Boards](http://azure.com/boards)
- The endpoint processes the request by taking the work item id and automatically assigning the work item and adds a tag

## How it works
- Create a webhook for work item create event and send request to your REST API endpoint. [See documentation](https://docs.microsoft.com/en-us/azure/devops/service-hooks/services/webhooks?view=azure-devops)
- The REST endpoint will accept the content and will turn around to update the given work item with an assigned to value and tag

## Configuration

The appsettings.json values need to be set with an Azure DevOps organization name and Personal Access Token (PAT)

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "AzureDevOpsOrgUrl": "https://dev.azure.com/<org name>",
    "AzureDevOpsToken": "<personal access token>"
  }

}
```
