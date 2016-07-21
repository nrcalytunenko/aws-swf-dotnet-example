namespace SwfCore
{
    public class WorkflowInfo
    {
        public string Domain { get; set; }
        public string DomainDescr { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string DeciderTaskList { get; set; }

        public static WorkflowInfo AlWorkflow = new WorkflowInfo
        {
            Domain = "alyt-test-domain",
            DomainDescr = "Demo domain for .Net SWF SDk",
            Name = "ALyt Test Workflow",
            Version = "1.0",
            DeciderTaskList = "ALytTest"
        };
    }

}
