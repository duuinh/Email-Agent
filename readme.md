# InboxFlow
Submitted for [CWB Hackathon 2025](https://www.cwbhackathon.com/problem-statements/mlai-intelligent-email-service-handling)

Automate and enhance the handling of incoming customer service emails in a banking environment using modular AI-powered agents that collaborate intelligently to streamline operations.

## Overview
<p align="center">
  <img src="https://github.com/user-attachments/assets/c4ad74bb-f416-4326-ac1b-a51db2caa6be" width="200"/>
</p>
The solution uses Azure Logic Apps to automate the end-to-end workflow of handling service requests via email. The steps are:

1. **Trigger:** When a new email arrives in the monitored inbox (`When a new email arrives (V2)`).
2. **Processing:** The app invokes the EmailProcessor Azure Function (`service-request-handling-agent-EmailProcessor`), which analyzes the email content, determines intent, extracts data, and interacts with the SR system.
3. **Response:** A reply email is automatically sent to the original sender (`Send an email (V2)`).

## Multi-Agent Orchestration

The system uses the Handoff Orchestration pattern in Microsoft Semantic Kernel. Each agent has a focused role, and agents hand off control to the next stage in the workflow.

### Agent Responsibilities

* **EmailTriageAgent**
  Classifies the incoming email to determine if it’s a new service request or a follow-up.

* **SRCreatorAgent**
  Handles service request creation based on email content. Always passes control to ReplyGeneratorAgent.

* **FollowUpHandlerAgent**
  Checks SR status for follow-up emails. Always passes control to ReplyGeneratorAgent.

* **ReplyGeneratorAgent**
  Generates the final customer reply using structured logic depending on the SR state (created, in progress, closed, or not found).

### Orchestration Flow
```
[EmailTriageAgent]
   ├──> [SRCreatorAgent] ─────┐
   └──> [FollowUpHandlerAgent] ───┐
                                  └──> [ReplyGeneratorAgent]
```
This modular pipeline enables intelligent, dynamic agent collaboration to automate decision-making and response generation in customer service workflows.

## Scope 
* Read and classify customer emails using LLM.
* Automatically create service requests via API.
* Reply with personalized responses using LLM.
* Handle follow-ups by checking SR status and sending updates.
* Orchestrate the flow using Semantic Kernel deployed on Azure Functions.
  
## Tech Stack

* Microsoft Semantic Kernel
* Azure Functions (.NET)
* Azure Logic Apps
* LLM (Azure OpenAI GPT-4.1-nano)
* GitHub API
* Azure Application Insights
  
## Demo
[Demo Video: Intelligent Service Request Handling in Action]

## Try It Yourself!
Send a service request email to our monitored inbox at:
`duuinh [at] outlook [dot] com`

Try sending:
* A new service request (e.g., request a new credit card, report a lost card)
* A follow-up email asking for status updates on your existing requests

Watch how you get a personalized response — powered by AI agents working behind the scenes!
