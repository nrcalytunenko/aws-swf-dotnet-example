using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using SwfCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SwfResults
{
    class Program
    {
        static void Main(string[] args)
        {
            var swfClient = new AmazonSimpleWorkflowClient();

            CheckDomainStatistic(swfClient, WorkflowInfo.AlWorkflow.Domain, DateTime.Now.AddDays(-20), DateTime.Now);

            CheckWorkflowStatusById(swfClient, WorkflowInfo.AlWorkflow.Domain, "AlytTestWF-636043422296483409", "22zlm6NvOLwxTRcVMCTidktLIbibeY/lU1FahuxXEcZIU=");

            CheckWorkflowExecutionHistory(swfClient, WorkflowInfo.AlWorkflow.Domain, "AlytTestWF-636043422296483409", "22zlm6NvOLwxTRcVMCTidktLIbibeY/lU1FahuxXEcZIU=");


            Console.Read();

        }

        private static void CheckWorkflowStatusById(IAmazonSimpleWorkflow swfClient, string domain, string workflowId, string runId)
        {
            var response = swfClient.DescribeWorkflowExecution(new DescribeWorkflowExecutionRequest
            {
                Domain = domain,
                Execution = new WorkflowExecution
                {
                    WorkflowId = workflowId,
                    RunId = runId
                }
            });

            Console.WriteLine(string.Format("Exectution info for workflow:{0}, run:{1}", workflowId, runId));
            Console.WriteLine(string.Format("Exectution status: {0}", response.WorkflowExecutionDetail.ExecutionInfo.ExecutionStatus));
            Console.WriteLine(string.Format("Exectution start time: {0}", response.WorkflowExecutionDetail.ExecutionInfo.StartTimestamp));
            Console.WriteLine(string.Format("Exectution type: {0}", response.WorkflowExecutionDetail.ExecutionInfo.WorkflowType.Name));
            Console.WriteLine(string.Format("Exectution close status: {0}", response.WorkflowExecutionDetail.ExecutionInfo.CloseStatus));
            Console.WriteLine(string.Format("Exectution close time: {0}", response.WorkflowExecutionDetail.ExecutionInfo.CloseTimestamp));
            Console.WriteLine(string.Format("Exectution latest context: {0}", response.WorkflowExecutionDetail.LatestExecutionContext));
            Console.WriteLine(string.Format("Exectution latest activity time: {0}", response.WorkflowExecutionDetail.LatestActivityTaskTimestamp));
        }

        private static void CheckWorkflowExecutionHistory(IAmazonSimpleWorkflow swfClient, string domain, string workflowId, string runId)
        {
            var executionHistoryRequest = new GetWorkflowExecutionHistoryRequest
            {
                Domain = domain,
                MaximumPageSize = 1000,
                Execution = new WorkflowExecution
                {
                    WorkflowId = workflowId,
                    RunId = runId
                }
            };


            var executionHistoryResp = swfClient.GetWorkflowExecutionHistory(executionHistoryRequest);
            var hasMore = true;
            while (hasMore)
            {
                executionHistoryResp.History.Events.ForEach(evt =>
                    Console.WriteLine(string.Format("Workflow History Event: {0}({1})", evt.EventType, evt.EventId)));
                hasMore = executionHistoryRequest.NextPageToken != null;
                if (hasMore)
                {
                    executionHistoryRequest.NextPageToken = executionHistoryResp.History.NextPageToken;
                    executionHistoryResp = swfClient.GetWorkflowExecutionHistory(executionHistoryRequest);
                }
            }

        }

        private static void CheckDomainStatistic(IAmazonSimpleWorkflow swfClient, string domain, DateTime start, DateTime end)
        {
            var closedWorkflowExecutionsRequest = new ListClosedWorkflowExecutionsRequest
            {
                Domain = domain,
                StartTimeFilter = new ExecutionTimeFilter
                {
                    OldestDate = start,
                    LatestDate = end
                }
            };

            var wfInfos = new List<WorkflowExecutionInfo>();
            var hasNext = true;
            while(hasNext)
            {
                var closedWorkflows = swfClient.ListClosedWorkflowExecutions(closedWorkflowExecutionsRequest);

                wfInfos.AddRange(closedWorkflows.WorkflowExecutionInfos.ExecutionInfos);
                closedWorkflowExecutionsRequest.NextPageToken = closedWorkflows.WorkflowExecutionInfos.NextPageToken;
                hasNext = closedWorkflows.WorkflowExecutionInfos.NextPageToken != null;
            }

            Console.WriteLine(string.Format("Total executions count:{0}", wfInfos.Count()));
            Console.WriteLine(string.Format("Total executions succeeded:{0}", wfInfos.Count(i => i.CloseStatus == CloseStatus.COMPLETED)));
            Console.WriteLine(string.Format("Total executions failed:{0}", wfInfos.Count(i =>  i.CloseStatus == CloseStatus.FAILED || i.CloseStatus == CloseStatus.CANCELED || i.CloseStatus == CloseStatus.TERMINATED || i.CloseStatus == CloseStatus.TIMED_OUT)));
            Console.WriteLine(string.Format("Average execution time (s):{0}",wfInfos.Average(i => (i.CloseTimestamp - i.StartTimestamp).Seconds)));
            
        }
    }
}
