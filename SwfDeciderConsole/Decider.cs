using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using SwfCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SwfDeciderConsole
{
    class Decider
    {
        private WorkflowInfo workflowInfo;

        public Decider(WorkflowInfo workflowInfo)
        {
            this.workflowInfo = workflowInfo;
        }

        public List<Decision> GetNext(DecisionTask decisionTask)
        {
            int pendingActivities = decisionTask.Events.Count(evt => evt.EventType == "Activity");
            int completedActivities = decisionTask.Events.Count(evt => evt.EventType == "ActivityTaskCompleted");
            int failedActivities = decisionTask.Events.Count(evt => evt.EventType.Value.Contains("Fail"));

            Console.WriteLine("Pending Activity Count=" + pendingActivities);
            Console.WriteLine("Completed Activity Count=" + completedActivities);

            List<Decision> decisions = new List<Decision>();

            if (failedActivities != 0)
            {
                Decision decision = new Decision()
                {
                    DecisionType = DecisionType.FailWorkflowExecution,
                    FailWorkflowExecutionDecisionAttributes =
                      new FailWorkflowExecutionDecisionAttributes
                      {
                          Reason = "Some activities have failed"
                      }
                };
                decisions.Add(decision);
            }
            else if (completedActivities == 0 && pendingActivities == 0) // Create this only at the begining
            {
                ScheduleActivity("ExtractDataA", decisions);
                ScheduleActivity("ExtractDataB", decisions);
            }
            else if (completedActivities == 2 && pendingActivities == 0)
            {
                ScheduleActivity("RunProcessing", decisions);
            }
            else if (completedActivities == 3 && pendingActivities == 0)
            {
                ScheduleActivity("ImportResults", decisions);
            }

            else if (completedActivities == 4)
            {
                Decision decision = new Decision()
                {
                    DecisionType = DecisionType.CompleteWorkflowExecution,
                    CompleteWorkflowExecutionDecisionAttributes =
                      new CompleteWorkflowExecutionDecisionAttributes
                      {
                          Result = "{\"Result\":\"WF Complete!\"}"
                      }
                };
                decisions.Add(decision);

                Console.WriteLine("Decider: WORKFLOW COMPLETE!!!!!!!!!!!!!!!!!!!!!!");
            }

            return decisions;
        }

        private void ScheduleActivity(string name, List<Decision> decisions)
        {
            Decision decision = new Decision()
            {
                DecisionType = DecisionType.ScheduleActivityTask,
                ScheduleActivityTaskDecisionAttributes =  // Uses DefaultTaskList
                  new ScheduleActivityTaskDecisionAttributes()
                  {
                      ActivityType = new ActivityType()
                      {
                          Name = name,
                          Version = workflowInfo.Version
                      },
                      ActivityId = name + "-" + System.Guid.NewGuid().ToString(),
                      Input = "{\"activityInput1\":\"value1\"}"
                  }
            };
            Console.WriteLine("Decider: ActivityId=" +
                          decision.ScheduleActivityTaskDecisionAttributes.ActivityId);
            decisions.Add(decision);
        }

    }
}
