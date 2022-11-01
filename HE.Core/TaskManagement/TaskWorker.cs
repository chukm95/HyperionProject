using HE.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.TaskManagement
{
    internal class TaskWorker
    {
        public bool IsAwake
        {
            get => Volatile.Read(ref isAwake);
        }

        private readonly int id;
        private readonly ConcurrentQueue<ITask> taskQueue;

        private Thread workerThread;
        private AutoResetEvent autoResetEvent;

        private bool isRunning;
        private bool isAwake;

        public TaskWorker(int id, ConcurrentQueue<ITask> taskQueue)
        {
            this.id = id;
            this.taskQueue = taskQueue;

            workerThread = new Thread(WorkLoop);
            workerThread.Start();

            autoResetEvent = new AutoResetEvent(false);

            isRunning = true;
            isAwake = true;
        }

        private void WorkLoop()
        {
            LogHandle logHandle = Logger.CreateLogHandle();
            logHandle.WriteInfo($"TaskWorker[{id}]", "Taskworker up and running!");

            while(isRunning)
            {
                ITask task;

                if(taskQueue.TryDequeue(out task))
                {
                    task.Execute(logHandle);
                }
                else
                {
                    isAwake = false;
                    autoResetEvent.WaitOne();
                    isAwake = true;
                }
            }

            logHandle.WriteInfo($"TaskWorker[{id}]", "Taskworker has stopped!");
        }

        public void Wake()
        {
            autoResetEvent.Set();
        }

        public void Stop()
        {
            Volatile.Write(ref isRunning, false);
            autoResetEvent.Set();
            workerThread.Join();
        }
    }
}
