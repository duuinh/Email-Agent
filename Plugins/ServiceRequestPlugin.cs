using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace EmailAgent.Plugins
{
    public sealed class ServiceRequestPlugin
    {
        [KernelFunction, Description("Creates a service request (SR) based on the email content.")]
        public string CreateServiceRequest(string emailContent) =>
            $"Service Request created for email: {emailContent}";

        [KernelFunction, Description("Gets the status of a service request.")]
        public string GetServiceRequestStatus(string srId) =>
            $"Status of Service Request {srId} is: In Progress";
    }
}