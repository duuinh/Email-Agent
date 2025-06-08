using System.ComponentModel;
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
            createIssue.Body = email.Body;
            createIssue.Labels.Add(category.GetDisplayName());

            var issue = await _client.Issue.Create("duuinh", "email-agent", createIssue);
            return $"Service Request created with number: {issue.Number}";
        }

        [KernelFunction, Description("Gets the status of a service request (SR)")]
        public async Task<string> GetServiceRequestStatusAsync(int srNumber) {
            try
            {
                var issue = await _client.Issue.Get("duuinh", "email-agent", srNumber);
                if (issue.State.Value == ItemState.Closed)
                {   
                    return $"SR#{srNumber} is resolved and closed.";
                }
                if (issue.Comments > 0)
                {
                    var comments = await _client.Issue.Comment.GetAllForIssue("duuinh", "email-agent", srNumber);
                    var lastComment = comments.LastOrDefault();
                    if (lastComment != null)
                    {
                        return $"SR#{srNumber} is being processed. Last Update: {lastComment.Body}";
                    }
                }
                return $"SR#{srNumber} is being processed. No updates available.";
            }
            catch (Exception ex)
            {
                return $"Error retrieving SR status: {ex.Message}";
            }
        }
    }
}