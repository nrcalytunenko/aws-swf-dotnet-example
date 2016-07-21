using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using SwfCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwfDeciderConsole
{
    class Program
    {
        static WorkflowInfo workflowInfo = WorkflowInfo.AlWorkflow;
        static string domainName = workflowInfo.Domain;
        static IAmazonSimpleWorkflow swfClient = new AmazonSimpleWorkflowClient();

        public static void Main(string[] args)
        {
            Task.Run(() => Decider(new Decider(workflowInfo))).Wait();
        }


        // Simple logic
        //  Creates four activities at the begining
        //  Waits for them to complete and completes the workflow
        static void Decider(Decider decider)
        {
            IAmazonSimpleWorkflow swfClient = new AmazonSimpleWorkflowClient();
            while (true)
            {
                Console.WriteLine("Decider: Polling for decision task ...");
                PollForDecisionTaskRequest request = new PollForDecisionTaskRequest()
                {
                    Domain = domainName,
                    TaskList = new TaskList() { Name = workflowInfo.DeciderTaskList }
                };

                PollForDecisionTaskResponse response = swfClient.PollForDecisionTask(request);
                if (response.DecisionTask.TaskToken == null)
                {
                    Console.WriteLine("Decider: Not tasks in a queue");
                    continue;
                }

                var decisions = decider.GetNext(response.DecisionTask);

                RespondDecisionTaskCompletedRequest respondDecisionTaskCompletedRequest =
                    new RespondDecisionTaskCompletedRequest()
                    {
                        Decisions = decisions,
                        TaskToken = response.DecisionTask.TaskToken
                    };
                swfClient.RespondDecisionTaskCompleted(respondDecisionTaskCompletedRequest);
            }
        }
    }
}
