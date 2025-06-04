using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace EmailAgent.Plugins
{
    public sealed class EmailPlugin
    {
        [KernelFunction, Description("Writes a response to an email.")]
        public string WriteResponse(string email, string subject, string response) =>
            $"""
                To: {email}
                Subject: Re: {subject}
                Body: {response}
            """;
    }
}