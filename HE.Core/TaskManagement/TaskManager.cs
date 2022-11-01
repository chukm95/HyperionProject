using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.TaskManagement
{
    public class TaskManager
    {
        private int coreCount;
        private ConcurrentQueue<ITask> taskQueue;

        private TaskWorker[] workers;

        internal TaskManager()
        {
            coreCount = 8;

            taskQueue = new ConcurrentQueue<ITask>();

            //create taskworkers
            workers = new TaskWorker[coreCount];
            for (int i = 0; i < coreCount; i++)
                workers[i] = new TaskWorker(i, taskQueue);

            Core.LogHandle.WriteInfo("Initialization", "TaskManager initialized!");
        }

        public void QueueTask(params ITask[] tasks)
        {
            for (int i = 0; i < tasks.Length; i++)
                taskQueue.Enqueue(tasks[i]);

            for (int x = 0; x < coreCount; x++)
            {
                if (!workers[x].IsAwake)
                {
                    workers[x].Wake();
                }
            }
        }

        internal void Deinitialize()
        {
            for (int i = 0; i < coreCount; i++)
                workers[i].Stop();

            Core.LogHandle.WriteInfo("Deinitialization", "TaskManager deinitialized!");
        }
    }
}
