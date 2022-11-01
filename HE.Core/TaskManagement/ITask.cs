using HE.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.TaskManagement
{
    public interface ITask
    {
        internal void Execute(LogHandle logHandle)
        {
            OnExecution(logHandle);
        }

        void OnExecution(LogHandle logHandle);
    }
}
