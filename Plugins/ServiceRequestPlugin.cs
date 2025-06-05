using System.ComponentModel;
using EmailAgent.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Octokit;

namespace EmailAgent.Plugins
{
    public sealed class ServiceRequestPlugin
    {
        private readonly GitHubClient _client;
        public ServiceRequestPlugin(GitHubClient client)
        {
            _client = client;
        }

        [KernelFunction, Description("Creates a service request (SR) based on the email content.")]
        public async Task<string> CreateServiceRequestAsync(Email email, ServiceRequestCategory category)
        {
            var createIssue = new NewIssue(email.Subject);
            createIssue.Body = $"{email.Body} \n" +
                               $"Customer: {email.From}";
            createIssue.Labels.Add(category.GetDisplayName());

            var issue = await _client.Issue.Create("duuinh", "email-agent", createIssue);
            return $"Service Request created with number: {issue.Number}";
        }

        [KernelFunction, Description("Gets the status of a service request.")]
        public async Task<string> GetServiceRequestStatusAsync(int srNumber) {
            var issue = await _client.Issue.Get("duuinh", "email-agent", srNumber);
            return $"Status of Service Request {srNumber} is: {issue.State.Value}"; 
        }
    }
}