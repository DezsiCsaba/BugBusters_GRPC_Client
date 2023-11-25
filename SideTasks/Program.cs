using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        int tasksNum = int.Parse(Console.ReadLine());
        List<Task> tasks = new List<Task>();

        for (int i = 0; i < tasksNum; i++)
        {
            string[] input = Console.ReadLine().Split(',');
            int Te = int.Parse(input[0]);
            int Tp = int.Parse(input[1]);
            tasks.Add(new Task(i, Te, Tp));
        }
        tasks.Sort();

        int elapsedTime = 0;
        List<int> processed = new List<int>();

        while (processed.Count < tasksNum)
        {
            List<Task> availableTasks = tasks.Where(t => t.Start <= elapsedTime && !processed.Contains(t.TaskIdx)).ToList();

            if (availableTasks.Count() > 0)
            {
                Task nextTask = availableTasks.OrderBy(t => t.Process).ThenBy(t => t.TaskIdx).First();
                elapsedTime += nextTask.Process;
                processed.Add(nextTask.TaskIdx);
            }
            else
            {
                elapsedTime++;
            }
        }
        Console.WriteLine(string.Join(",", processed));
    }
}

class Task : IComparable<Task>
{
    public int TaskIdx { get; }
    public int Start { get; }
    public int Process { get; }
    public Task(int index, int start, int process)
    {
        TaskIdx = index;
        Start = start;
        Process = process;
    }

    public int CompareTo(Task other)
    {
        if (this.Start != other.Start)
        {
            return (this.Start > other.Start) ? 1:0;
        }
        return (this.Process > other.Process) ? 1:0;
    }
}