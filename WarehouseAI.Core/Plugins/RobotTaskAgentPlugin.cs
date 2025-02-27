using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace WarehouseAI.Core.Plugins;

public class RobotTaskAgentPlugin
{
    private readonly List<Robot> robots =
    [
        new Robot("R1", "Idle"),
        new Robot("R2", "Idle"),
        new Robot("R3", "Idle"),
        new Robot("R4", "Idle"),
    ];

    private readonly Queue<TaskAssignment> taskQueue = new();

    [
        KernelFunction("AssignTask"),
        Description(
            "Assigns a task to an available robot. If no robots are available, the task is added to the queue."
        )
    ]
    public string AssignTask(
        [Description("The task that needs to be assigned.")] string task,
        [Description("The priority of the task (Low, Medium, High).")] string priority
    )
    {
        var availableRobot = robots.FirstOrDefault(r => r.Status == "Idle");

        if (availableRobot != null)
        {
            availableRobot.Status = "Busy";
            availableRobot.CurrentTask = task;
            return $"✅ Robot {availableRobot.Id} assigned to '{task}' (Priority: {priority}).";
        }

        taskQueue.Enqueue(new TaskAssignment(task, priority));
        return $"⚠️ No available robots! Task '{task}' (Priority: {priority}) added to the queue.";
    }

    [
        KernelFunction("GetRobotStatuses"),
        Description("Retrieves the current status of all robots, including their assigned tasks.")
    ]
    public string GetRobotStatuses()
    {
        var report = string.Join(
            "\n",
            robots.Select(r => $"🤖 {r.Id}: {r.Status} (Task: {r.CurrentTask ?? "None"})")
        );
        return $"📊 **Robot Statuses:**\n{report}";
    }

    [
        KernelFunction("CompleteTask"),
        Description(
            "Marks the task of a specified robot as complete. If a queued task exists, assigns it to the robot."
        )
    ]
    public string CompleteTask(
        [Description("The ID of the robot completing its task.")] string robotId
    )
    {
        var robot = robots.FirstOrDefault(r => r.Id == robotId);
        if (robot == null)
        {
            return $"❌ Robot {robotId} not found!";
        }

        robot.Status = "Idle";
        robot.CurrentTask = null;

        if (taskQueue.Count > 0)
        {
            var nextTask = taskQueue.Dequeue();
            robot.Status = "Busy";
            robot.CurrentTask = nextTask.Description;
            return $"✅ Robot {robot.Id} is now assigned to queued task '{nextTask.Description}' (Priority: {nextTask.Priority}).";
        }

        return $"✅ Robot {robot.Id} is now available.";
    }

    private class Robot(string id, string status)
    {
        public string Id { get; } = id;
        public string Status { get; set; } = status;
        public string CurrentTask { get; set; }
    }

    private class TaskAssignment(string description, string priority)
    {
        public string Description { get; } = description;
        public string Priority { get; } = priority;
    }
}