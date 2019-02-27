# azure-devops-work-items-webhook-sample
Simple example of a Webhook receiver from Azure Boards work item create event

## How it works
- Create a webhook for work item create and send request to REST API endpoint. [See documentation](https://docs.microsoft.com/en-us/azure/devops/service-hooks/services/webhooks?view=azure-devops)
- The REST endpoint will accept the content and will turn around to update the given work item with an assigned to value and tag


