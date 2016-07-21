using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using SwfCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwfActivitiesConsole
{
    class Program
    {
        static WorkflowInfo workflowInfo = WorkflowInfo.AlWorkflow;
        static string domainName = workflowInfo.Domain;

        public static void Main(string[] args)
        {
            string workflowName = workflowInfo.Name;

            Task.Run(() => Worker("Extract"));
            Task.Run(() => Worker("Extract"));
            Task.Run(() => Worker("Processing"));
            Task.Run(() => Worker("Import"));
            Task.Run(() => Worker("Notification"));

            Console.Read();
        }
        
        static void Worker(string tasklistName)
        {
            var swfClient = new AmazonSimpleWorkflowClient();
            string prefix = string.Format("Worker{0}:{1:x} ", tasklistName,
                                  System.Threading.Thread.CurrentThread.ManagedThreadId);
            while (true)
            {
                Console.WriteLine(prefix + ": Polling for activity task ...");
                PollForActivityTaskRequest pollForActivityTaskRequest =
                    new PollForActivityTaskRequest()
                    {
                        Domain = domainName,
                        TaskList = new TaskList()
                        {
                            // Poll only the tasks assigned to me
                            Name = tasklistName
                        }
                    };

                PollForActivityTaskResponse pollForActivityTaskResponse =
                                swfClient.PollForActivityTask(pollForActivityTaskRequest);

                RespondActivityTaskCompletedRequest respondActivityTaskCompletedRequest =
                            new RespondActivityTaskCompletedRequest()
                            {
                                Result = "{\"activityResult1\":\"Result Value1\"}",
                                TaskToken = pollForActivityTaskResponse.ActivityTask.TaskToken
                            };

                if (pollForActivityTaskResponse.ActivityTask.ActivityId == null)
                {
                    Console.WriteLine(prefix + ": NULL");
                }
                else
                {
                    RespondActivityTaskCompletedResponse respondActivityTaskCompletedResponse =
                        swfClient.RespondActivityTaskCompleted(respondActivityTaskCompletedRequest);
                    Console.WriteLine(prefix + ": Activity task completed. ActivityId - " +
                        pollForActivityTaskResponse.ActivityTask.ActivityId);
                }
            }
        }
    }
}
