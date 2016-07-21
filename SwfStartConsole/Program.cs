using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using SwfCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SwfStartConsole
{
    class Program
    {
        static WorkflowInfo workflowInfo = WorkflowInfo.AlWorkflow;
        static string domainName = workflowInfo.Domain;
        static IAmazonSimpleWorkflow swfClient = new AmazonSimpleWorkflowClient();

        public static void Main(string[] args)
        {
            string workflowName = workflowInfo.Name;

            // Setup
            RegisterDomain();
            RegisterActivity("ExtractDataA", "Extract");
            RegisterActivity("ExtractDataB", "Extract");
            RegisterActivity("RunProcessing", "Processing");
            RegisterActivity("ImportResults", "Import");
            RegisterActivity("Notify", "Notification");
            RegisterWorkflow(workflowName);

            Task.Run(() => StartWorkflow(workflowName));

            Console.Read();
        }

        static void RegisterDomain()
        {
            // Register if the domain is not already registered.
            var listDomainRequest = new ListDomainsRequest()
            {
                RegistrationStatus = RegistrationStatus.REGISTERED
            };

            if (swfClient.ListDomains(listDomainRequest).DomainInfos.Infos.FirstOrDefault(
                                                      x => x.Name == domainName) == null)
            {
                RegisterDomainRequest request = new RegisterDomainRequest()
                {
                    Name = domainName,
                    Description = workflowInfo.DomainDescr,
                    WorkflowExecutionRetentionPeriodInDays = "10"
                };

                Console.WriteLine("Setup: Created Domain - " + domainName);
                swfClient.RegisterDomain(request);
            }
        }

        static void RegisterActivity(string name, string tasklistName)
        {
            // Register activities if it is not already registered
            var listActivityRequest = new ListActivityTypesRequest()
            {
                Domain = domainName,
                Name = name,
                RegistrationStatus = RegistrationStatus.REGISTERED
            };

            if (swfClient.ListActivityTypes(listActivityRequest).ActivityTypeInfos.TypeInfos.FirstOrDefault(
                                          x => x.ActivityType.Version == workflowInfo.Version) == null)
            {
                RegisterActivityTypeRequest request = new RegisterActivityTypeRequest()
                {
                    Name = name,
                    Domain = domainName,
                    Description = ".Net Demo Activities",
                    Version = workflowInfo.Version,
                    DefaultTaskList = new TaskList() { Name = tasklistName },//Worker poll based on this
                    DefaultTaskScheduleToCloseTimeout = "300",
                    DefaultTaskScheduleToStartTimeout = "150",
                    DefaultTaskStartToCloseTimeout = "450",
                    DefaultTaskHeartbeatTimeout = "NONE"
                };

                swfClient.RegisterActivityType(request);
                Console.WriteLine("Setup: Created Activity Name - " + request.Name);
            }
        }

        static void RegisterWorkflow(string name)
        {
            // Register workflow type if not already registered
            var listWorkflowRequest = new ListWorkflowTypesRequest()
            {
                Name = name,
                Domain = domainName,
                RegistrationStatus = RegistrationStatus.REGISTERED
            };
            if (swfClient.ListWorkflowTypes(listWorkflowRequest).WorkflowTypeInfos.TypeInfos.FirstOrDefault(
                                            x => x.WorkflowType.Version == workflowInfo.Version) == null)
            {
                RegisterWorkflowTypeRequest request = new RegisterWorkflowTypeRequest()
                {
                    DefaultChildPolicy = ChildPolicy.TERMINATE,
                    DefaultExecutionStartToCloseTimeout = "300",
                    DefaultTaskList = new TaskList()
                    {
                        Name = workflowInfo.DeciderTaskList // Decider need to poll for this task
                    },
                    DefaultTaskStartToCloseTimeout = "150",
                    Domain = domainName,
                    Name = name,
                    Version = workflowInfo.Version
                };

                swfClient.RegisterWorkflowType(request);

                Console.WriteLine("Setup: Registerd Workflow Name - " + request.Name);
            }
        }

        static void StartWorkflow(string name)
        {
            var swfClient = new AmazonSimpleWorkflowClient();
            string workflowID = "AlytTestWF-" + DateTime.Now.Ticks;
            swfClient.StartWorkflowExecution(new StartWorkflowExecutionRequest()
            {
                Input = "{\"inputparam1\":\"value1\"}", // Serialize input to a string

                WorkflowId = workflowID,
                Domain = domainName,
                WorkflowType = new WorkflowType()
                {
                    Name = name,
                    Version = workflowInfo.Version
                }
            });
            Console.WriteLine("Setup: Workflow Instance created ID=" + workflowID);
        }
    }
}
